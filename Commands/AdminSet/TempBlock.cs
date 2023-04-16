using System.Net;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.AdminSet;
internal sealed class TempBlock : Command
{
    public override CommandInfo Info { get; } = new(
        name: "tempblock",
        permission: PermissionLevels.Whitelisted
    );

    private readonly string _reqUrl = AppConfigLoader.Config.BlocksLink;
    private readonly HttpClient _requests = new()
    {
        DefaultRequestHeaders =
        {
            { "Authorization", $"Basic {AppConfigLoader.Config.BlocksAuth}" }
        }
    };

    public override async Task Execute(CommandContext ctx)
    {
        string channel = ctx.Message.Channel.Name;
        string user = ctx.Message.Author.DisplayName;
        string[] args = ctx.Args;

        if (args.Length < 2)
            return;

        if (!int.TryParse(args[1], out int i))
        {
            await MessageHandler.SendMessage(channel, $"@{user}, {args[1]} could not be converted to int");
            return;
        }

        IvrUser getTarget = await ExternalApiHandler.GetIvrUser(args[0]);
        if (getTarget is null)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, User could not be retrieved through Ivr.fi");
            return;
        }

        HttpResponseMessage response;
        try
        {
            response = await _requests.PutAsync($"{_reqUrl}/add?targetId={getTarget.Id}&hours={args[1]}", null);
        }
        catch (TaskCanceledException)
        {
            Log.Error("[{h}] Blocking user {u} timed out.", nameof(TempBlock), args[0]);
            return;
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, User already blocked");
            return;
        }
        else if (!response.IsSuccessStatusCode)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, {response.StatusCode}");
            return;
        }

        await MessageHandler.SendMessage(channel, $"Blocked {args[0]}  until {DateTime.Now.AddHours(i):HH:mm:ss dd.MM.yyyy}");
    }
}
