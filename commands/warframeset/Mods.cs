﻿using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.json;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.warframeset;
internal class Mods : DataCacher<ModInfo>, IChatCommand
{
    public Command Info()
    {
        string name = "modinfo";
        string description = "Get the stats of the closest matching mod";
        string[] aliases = { "mod", "mods" };
        int[] cooldowns = { 5, 3 };

        return new Command(name, description, aliases, cooldowns);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Specify the mod you're looking for FeelsDankMan");
            return;
        }

        string modName = string.Join(' ', args).ToLower();
        ModInfo? mod = GetCachedPiece(modName)?.Object
            ?? await ExternalAPIHandler.GetModInfo(modName);
        if (mod is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, An error occured with your request :(");
            return;
        }

        int level = 0;
        string modString = $"{mod.type} \"{mod.name}\" [Rank:{level}/{mod.fusionLimit}] " +
            $"-- drain:{mod.baseDrain + level} " +
            $"-- {string.Join(" | ", mod.levelStats[level].stats)} " +
            $"{(mod.tradable ? string.Empty : "(untradable)")}";

        MessageHandler.SendMessage(channel, $"@{user}, {modString}");
        CachePiece(modName, mod, 150);
    }
}
