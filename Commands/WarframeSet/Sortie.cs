﻿using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal sealed class Sortie : Command
{
    public override CommandInfo Info { get; } = new(
        name: "sortie",
        description: "Check the current Sortie",
        aliases: new string[] { "anasa", "sorties" },
        userCooldown: 5,
        channelCooldown: 3
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;

        (bool keyExists, CurrentSortie value) = await Redis.Cache.TryGetObjectAsync<CurrentSortie>("warframe:sortiedata");
        if (!keyExists)
        {
            Result<CurrentSortie> r = await ExternalApiHandler.WarframeStatusApi<CurrentSortie>("sortie");
            if (!r.Success)
            {
                await MessageHandler.SendMessage(channel, $"@{user}, Failed to fetch the current sortie. ({r.Exception.Message})");
                return;
            }

            await Redis.Cache.SetObjectAsync("warframe:sortiedata", r.Value, Time.Until(r.Value.Expiry));
            value = r.Value;
        }

        CurrentSortie sortie = value;

        if (Time.HasPassed(sortie.Expiry))
        {
            _ = await Redis.Cache.RemoveAsync("warframe:sortiedata");
            await MessageHandler.SendMessage(channel, $"@{user}, Sortie data is outdated. You should try again later ppL");
            return;
        }

        string sortieString = $"{sortie.Faction} " +
            $"➜ ● {sortie.Variants[0].MissionType} [{ModifierOf(sortie.Variants[0])}] " +
            $"➜ ■ {sortie.Variants[1].MissionType} [{ModifierOf(sortie.Variants[1])}] " +
            $"➜ ◆ {(sortie.Variants[2].MissionType == "Assassination" ? $"{sortie.Boss} Assassination" : sortie.Variants[2].MissionType)} [{ModifierOf(sortie.Variants[2])}]";

        await MessageHandler.SendMessage(channel, $"@{user}, {sortieString} -- time left: {Time.UntilString(sortie.Expiry)}");
    }

    private string ModifierOf(Variant variant)
    {
        string[] split = variant.Modifier.Split(": ");
        return split[0] switch
        {
            "Eximus Stronghold" => "+Eximus",
            "Weapon Restriction" => split[1],
            "Augmented Enemy Armor" => "Augmented Armor 🪨", // rock emoji
            "Enhanced Enemy Shields" => "Enhanced Shields 🛡",
            "Enemy Elemental Enhancement" => split[1] switch
            {
                "Heat" => "+🔥",
                "Cold" => "+❄",
                "Electricity" => "+⚡",
                "Magnetic" => "+🧲",
                "Blast" => "+💥",
                "Radiation" => "+☢",
                "Viral" => "+🦠",
                "Corrosive" => "+🧪",
                // Gas
                // Toxin
                _ => '+' + split[1]
            },
            "Enemy Physical Enhancement" => split[1] switch
            {
                "Puncture" => "+Puncture 📌",
                "Slash" => "+Slash 🔪",
                "Impact" => "+Impact 🔨",
                _ => split[1]
            },
            "Environmental Hazard" => split[1] switch
            {
                "Radiation Pockets" => "Radiation Pockets ItsBoshyTime ",
                "Fire" => variant.Modifier + " 🔥",
                _ => split[1]
            },
            "Environmental Effect" => split[1] switch
            {
                "Cryogenic Leakage" => "Cryogenic Leakage 🥶",
                "Electromagnetic Anomalies" => "Electromagnetic Anomalies 🧲 👻",
                _ => split[1]
            },
            // Energy Reduction
            _ => split[0]
        };
    }
}
