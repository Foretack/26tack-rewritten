﻿using SqlKata.Execution;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.BaseSet;
internal sealed class RandomMidjourney : Command
{
    public override CommandInfo Info { get; } = new(
        name: "midjourney",
        description: "Retrieve a random image generated by Midjourney",
        aliases: new string[] { "rmj", "mj" },
        10, 5,
        PermissionLevels.Everyone
    );

    public override async Task Execute(CommandContext ctx)
    {
        string channel = ctx.IrcMessage.Channel;
        string user = ctx.IrcMessage.DisplayName;

        using var db = new DbQueries();
        IEnumerable<dynamic> query = await db.Enqueue(q => q.SelectRaw(
            $"* FROM midjourney_images " +
            $"OFFSET floor(random() * (SELECT COUNT(*) FROM midjourney_images)) " +
            $"LIMIT 1").GetAsync());

        dynamic row = query.FirstOrDefault();
        if (row is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, I could not fetch a random image PoroSad");
            return;
        }

        using var requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(10);

        byte[] bytes;
        try
        { bytes = await requests.GetByteArrayAsync(row.link); }
        catch
        {
            _ = await db.Enqueue("midjourney_images", q => q.Where("link", "=", $"{row.link}").DeleteAsync());
            MessageHandler.SendMessage(channel, "Fetched an image that no longer exists! Try again. PoroSad");
            return;
        }

        MultipartFormDataContent content = new()
        {
            { new ByteArrayContent(bytes), "file", $"image{Random.Shared.Next(1000)}.{row.link_ext}" }
        };

        requests.DefaultRequestHeaders.Add("Authorization", AppConfigLoader.Config.ImageHostAuth);
        HttpResponseMessage response = await requests.PostAsync(AppConfigLoader.Config.ImageHostLink, content);
        string responseString = await response.Content.ReadAsStringAsync();
        if (!responseString.Contains(AppConfigLoader.Config.ImageHostLink[..5]))
        {
            MessageHandler.SendMessage(channel, $"@{user}, Image could not be uploaded PoroSad");
            return;
        }

        string? ps = row.prompt as string;
        MessageHandler.SendMessage(channel, $"@{user}, \"{(ps?.EndsWith(' ') ?? false ? ps[..^1] : ps)}\" {responseString}");
    }
}