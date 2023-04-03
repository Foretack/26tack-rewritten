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
            await MessageHandler.SendMessage(channel, $"@{user}, Args[1] could not be converted to int");
            return;
        }

        Result<User> getTarget = await User.Get(args[0]);
        if (!getTarget.Success)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, User could not be retrieved through Helix");
            return;
        }

        HttpResponseMessage response = await _requests.PutAsync($"{_reqUrl}/add?targetId={getTarget.Value.Id}&hours={args[1]}", null);
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
