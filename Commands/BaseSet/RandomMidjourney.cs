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
        var query = await db.QueryFactory.Query().SelectRaw(
            $"* FROM midjourney_images " +
            $"OFFSET floor(random() * (SELECT COUNT(*) FROM midjourney_images)) " +
            $"LIMIT 1").GetAsync();

        var row = query.FirstOrDefault();
        if (row is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, I could not fetch a random image PoroSad");
            return;
        }

        using var requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(10);
        requests.DefaultRequestHeaders.Add("Authorization", AppConfigLoader.Config.ImageHostAuth);

        byte[] bytes = await requests.GetByteArrayAsync(row.link);
        MultipartFormDataContent content = new()
        {
            { new ByteArrayContent(bytes), "file", $"image{Random.Shared.Next(1000)}.{row.link_ext}" }
        };

        HttpResponseMessage response = await requests.PostAsync(AppConfigLoader.Config.ImageHostLink, content);
        string responseString = await response.Content.ReadAsStringAsync();
        if (!responseString.Contains("occluder.space"))
        {
            MessageHandler.SendMessage(channel, $"@{user}, Image could not be uploaded PoroSad");
            return;
        }

        MessageHandler.SendMessage(channel, $"@{user}, \"{row.prompt}\" {responseString}");
    }
}
