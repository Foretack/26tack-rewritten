﻿using System.Text.RegularExpressions;
using Tack.Core;
using Tack.Database;
using Tack.Nonclass;
using TwitchLib.Client.Events;

namespace Tack.Modules;
internal class LinkCollection : ChatModule
{
    public LinkCollection()
    {
        AnonymousClient.Client.OnMessageReceived += OnMessage;

        OnEnabled = _ => AnonymousClient.Client.OnMessageReceived += OnMessage;
        OnDisabled = _ => AnonymousClient.Client.OnMessageReceived -= OnMessage;
    }

    private static readonly Regex _regex = new(@"(https:[\\/][\\/])?(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\=]*)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

    private async void OnMessage(object? sender, OnMessageReceivedArgs e)
    {
        var ircMessage = e.ChatMessage;
        if (ircMessage.Message.Length < 10) return;
        if (ircMessage.Username.Contains("bot")) return;

        string? link = ircMessage.Message.Split(' ').FirstOrDefault(x => _regex.IsMatch(x));
        if (link is null) return;
        if (link.Length < 10) return;

        using (DbQueries db = new DbQueries())
        {
            _ = await db
                .Table("collected_links")
                .Insert()
                .Schema("username", "channel", "link_text")
                .Values(
                    $"'{ircMessage.Username}'",
                    $"'{ircMessage.Channel}'",
                    $"'{link}'"
                        )
                .TryExecute();
        }
    }
}
