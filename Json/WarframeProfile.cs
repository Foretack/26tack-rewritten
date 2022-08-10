using System.Text.Json.Serialization;

#pragma warning disable CS8618
#pragma warning disable IDE1006

namespace Tack.Json;

#region Token stuff
public class V5Root
{
    [JsonPropertyName("tokens")]
    public Token[] Tokens { get; set; }
}

public class Token
{
    [JsonPropertyName("extension_id")]
    public string ExtensionId { get; set; }

    [JsonPropertyName("token")]
    public string Key { get; set; }

    [JsonPropertyName("helix_token")]
    public string HelixToken { get; set; }
}
#endregion

#region Profile data
public record ProfileRoot(
    [property: JsonPropertyName("accountInfo")] AccountInfo AccountInfo /*,
    [property: JsonPropertyName("loadOuts")] LoadOuts LoadOuts  | I don't have any use for this data yet */
);

public record AccountInfo(
    [property: JsonPropertyName("focus")] string Focus,
    [property: JsonPropertyName("glyph")] string Glyph,
    [property: JsonPropertyName("lastUpdated")] int LastUpdated,
    [property: JsonPropertyName("masteryRank")] int MasteryRank,
    [property: JsonPropertyName("playerName")] string PlayerName
);

public record Amp(
    [property: JsonPropertyName("features")] int Features,
    [property: JsonPropertyName("focus")] string Focus,
    [property: JsonPropertyName("itemName")] string ItemName,
    [property: JsonPropertyName("modularParts")] ModularParts ModularParts,
    [property: JsonPropertyName("pricol")] Pricol Pricol,
    [property: JsonPropertyName("skins")] IReadOnlyList<Skin> Skins,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<Upgrade> Upgrades,
    [property: JsonPropertyName("xp")] int Xp
);

public record ARCHWING(
    [property: JsonPropertyName("archwing")] Archwing Archwing,
    [property: JsonPropertyName("melee")] Melee Melee,
    [property: JsonPropertyName("primary")] Primary Primary
);

public record Archwing(
    [property: JsonPropertyName("features")] int Features,
    [property: JsonPropertyName("pricol")] Pricol Pricol,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<Upgrade> Upgrades,
    [property: JsonPropertyName("xp")] int Xp
);

public record Armaments(
    [property: JsonPropertyName("modularParts")] ModularParts ModularParts
);

public record Attcol(
    [property: JsonPropertyName("en")] string En,
    [property: JsonPropertyName("m0")] string M0,
    [property: JsonPropertyName("m1")] string M1,
    [property: JsonPropertyName("t0")] string T0,
    [property: JsonPropertyName("t1")] string T1,
    [property: JsonPropertyName("t2")] string T2,
    [property: JsonPropertyName("t3")] string T3,
    [property: JsonPropertyName("e1")] string E1
);

public record COMBAT(
    [property: JsonPropertyName("Assigned")] int Assigned
);

public record Crew(
    [property: JsonPropertyName("members")] Members Members
);

public record Crewweapon(
    [property: JsonPropertyName("features")] int Features,
    [property: JsonPropertyName("polarized")] int Polarized,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<Upgrade> Upgrades,
    [property: JsonPropertyName("xp")] int Xp,
    [property: JsonPropertyName("itemName")] string ItemName,
    [property: JsonPropertyName("upgradeType")] string UpgradeType
);

public record DATAKNIFE(
    [property: JsonPropertyName("parazon")] Parazon Parazon,
    [property: JsonPropertyName("primary")] Primary Primary
);

public record ENGINEERING(
    [property: JsonPropertyName("Assigned")] int Assigned
);

public record ENGINES(
    [property: JsonPropertyName("uniqueName")] string UniqueName
);

public record Eyecol(
    [property: JsonPropertyName("en")] string En,
    [property: JsonPropertyName("m0")] string M0,
    [property: JsonPropertyName("m1")] string M1,
    [property: JsonPropertyName("t0")] string T0,
    [property: JsonPropertyName("t1")] string T1,
    [property: JsonPropertyName("t2")] string T2,
    [property: JsonPropertyName("t3")] string T3
);

public record Facial(
    [property: JsonPropertyName("t0")] string T0,
    [property: JsonPropertyName("t1")] string T1
);

public record GUNNERY(
    [property: JsonPropertyName("Assigned")] int Assigned
);

public record Haircol(
    [property: JsonPropertyName("t0")] string T0,
    [property: JsonPropertyName("t2")] string T2
);

public record Heavy(
    [property: JsonPropertyName("features")] int Features,
    [property: JsonPropertyName("polarized")] int Polarized,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<Upgrade> Upgrades,
    [property: JsonPropertyName("xp")] int Xp
);

public record HULL(
    [property: JsonPropertyName("uniqueName")] string UniqueName
);

public record LoadOuts(
    [property: JsonPropertyName("ARCHWING")] ARCHWING ARCHWING,
    [property: JsonPropertyName("DATAKNIFE")] DATAKNIFE DATAKNIFE,
    [property: JsonPropertyName("MECH")] MECH MECH,
    [property: JsonPropertyName("NORMAL")] NORMAL NORMAL,
    [property: JsonPropertyName("OPERATOR")] OPERATOR OPERATOR,
    [property: JsonPropertyName("RAILJACK")] RAILJACK RAILJACK,
    [property: JsonPropertyName("SENTINEL")] SENTINEL SENTINEL
);

public record Markings(
    [property: JsonPropertyName("en")] string En,
    [property: JsonPropertyName("t0")] string T0,
    [property: JsonPropertyName("t1")] string T1
);

public record MECH(

);

public record Melee(
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<Upgrade> Upgrades,
    [property: JsonPropertyName("xp")] int Xp,
    [property: JsonPropertyName("features")] int Features,
    [property: JsonPropertyName("pricol")] Pricol Pricol
);

public record Members(
    [property: JsonPropertyName("SLOT_A")] SLOTA SLOTA,
    [property: JsonPropertyName("SLOT_B")] SLOTB SLOTB,
    [property: JsonPropertyName("SLOT_C")] SLOTC SLOTC
);

public record ModularParts(
    [property: JsonPropertyName("LWPT_AMP_BRACE")] string LWPTAMPBRACE,
    [property: JsonPropertyName("LWPT_AMP_CORE")] string LWPTAMPCORE,
    [property: JsonPropertyName("LWPT_AMP_OCULUS")] string LWPTAMPOCULUS,
    [property: JsonPropertyName("ORDINANCE")] ORDINANCE ORDINANCE,
    [property: JsonPropertyName("PILOT")] PILOT PILOT,
    [property: JsonPropertyName("PORT_GUNS")] PORTGUNS PORTGUNS,
    [property: JsonPropertyName("ENGINES")] ENGINES ENGINES,
    [property: JsonPropertyName("HULL")] HULL HULL,
    [property: JsonPropertyName("REACTOR")] REACTOR REACTOR,
    [property: JsonPropertyName("SHIELD")] SHIELD SHIELD
);

public record NORMAL(
    [property: JsonPropertyName("heavy")] Heavy Heavy,
    [property: JsonPropertyName("melee")] Melee Melee,
    [property: JsonPropertyName("primary")] Primary Primary,
    [property: JsonPropertyName("secondary")] Secondary Secondary,
    [property: JsonPropertyName("warframe")] Warframe Warframe
);

public record OPERATOR(
[property: JsonPropertyName("amp")] Amp Amp,
    [property: JsonPropertyName("operator")] Operator Operator
);

public record Operator(
    [property: JsonPropertyName("attcol")] Attcol Attcol,
    [property: JsonPropertyName("eyecol")] Eyecol Eyecol,
    [property: JsonPropertyName("facial")] Facial Facial,
    [property: JsonPropertyName("haircol")] Haircol Haircol,
    [property: JsonPropertyName("markings")] Markings Markings,
    [property: JsonPropertyName("pricol")] Pricol Pricol,
    [property: JsonPropertyName("sigcol")] Sigcol Sigcol,
    [property: JsonPropertyName("skins")] IReadOnlyList<string> Skins,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<Upgrade> Upgrades
);

public record ORDINANCE(
    [property: JsonPropertyName("uniqueName")] string UniqueName
);

public record Parazon(
    [property: JsonPropertyName("pricol")] Pricol Pricol,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<Upgrade> Upgrades,
    [property: JsonPropertyName("xp")] int Xp
);

public record PILOT(
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgradeType")] string UpgradeType,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<object> Upgrades
);

public record PILOTING(
    [property: JsonPropertyName("Assigned")] int Assigned
);

public record PORTGUNS(
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgradeType")] string UpgradeType,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<object> Upgrades
);

public record Pricol(
    [property: JsonPropertyName("en")] string En,
    [property: JsonPropertyName("m0")] string M0,
    [property: JsonPropertyName("m1")] string M1,
    [property: JsonPropertyName("t0")] string T0,
    [property: JsonPropertyName("t1")] string T1,
    [property: JsonPropertyName("t2")] string T2,
    [property: JsonPropertyName("t3")] string T3,
    [property: JsonPropertyName("e1")] string E1
);

public record Primary(
    [property: JsonPropertyName("features")] int Features,
    [property: JsonPropertyName("polarized")] int Polarized,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<Upgrade> Upgrades,
    [property: JsonPropertyName("xp")] int Xp,
    [property: JsonPropertyName("pricol")] Pricol Pricol
);

public record RAILJACK(
    [property: JsonPropertyName("armaments")] Armaments Armaments,
    [property: JsonPropertyName("crew")] Crew Crew,
    [property: JsonPropertyName("railjack")] Railjack Railjack
);

public record Railjack(
    [property: JsonPropertyName("itemName")] string ItemName,
    [property: JsonPropertyName("modularParts")] ModularParts ModularParts,
    [property: JsonPropertyName("pricol")] Pricol Pricol,
    [property: JsonPropertyName("slotLevels")] SlotLevels SlotLevels,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<object> Upgrades
);

public record REACTOR(
    [property: JsonPropertyName("uniqueName")] string UniqueName
);

public record Secondary(
    [property: JsonPropertyName("features")] int Features,
    [property: JsonPropertyName("itemName")] string ItemName,
    [property: JsonPropertyName("polarized")] int Polarized,
    [property: JsonPropertyName("pricol")] Pricol Pricol,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgradeType")] string UpgradeType,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<Upgrade> Upgrades,
    [property: JsonPropertyName("xp")] int Xp
);

public record SENTINEL(

);

public record SHIELD(
    [property: JsonPropertyName("uniqueName")] string UniqueName
);

public record Sigcol(
    [property: JsonPropertyName("en")] string En,
    [property: JsonPropertyName("m1")] string M1,
    [property: JsonPropertyName("t2")] string T2,
    [property: JsonPropertyName("t3")] string T3,
    [property: JsonPropertyName("t0")] string T0
);

public record Skills(
    [property: JsonPropertyName("COMBAT")] COMBAT COMBAT,
    [property: JsonPropertyName("ENGINEERING")] ENGINEERING ENGINEERING,
    [property: JsonPropertyName("GUNNERY")] GUNNERY GUNNERY,
    [property: JsonPropertyName("PILOTING")] PILOTING PILOTING,
    [property: JsonPropertyName("SURVIVABILITY")] SURVIVABILITY SURVIVABILITY
);

public record Skin(
    [property: JsonPropertyName("uniqueName")] string UniqueName
);

public record SLOTA(
    [property: JsonPropertyName("attcol")] Attcol Attcol,
    [property: JsonPropertyName("crewweapon")] Crewweapon Crewweapon,
    [property: JsonPropertyName("eyecol")] Eyecol Eyecol,
    [property: JsonPropertyName("faction")] string Faction,
    [property: JsonPropertyName("pricol")] Pricol Pricol,
    [property: JsonPropertyName("role")] int Role,
    [property: JsonPropertyName("sigcol")] IReadOnlyList<object> Sigcol,
    [property: JsonPropertyName("skills")] Skills Skills,
    [property: JsonPropertyName("suit")] string Suit,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("xp")] int Xp
);

public record SLOTB(
    [property: JsonPropertyName("attcol")] Attcol Attcol,
    [property: JsonPropertyName("crewweapon")] Crewweapon Crewweapon,
    [property: JsonPropertyName("eyecol")] Eyecol Eyecol,
    [property: JsonPropertyName("faction")] string Faction,
    [property: JsonPropertyName("pricol")] Pricol Pricol,
    [property: JsonPropertyName("role")] int Role,
    [property: JsonPropertyName("sigcol")] IReadOnlyList<object> Sigcol,
    [property: JsonPropertyName("skills")] Skills Skills,
    [property: JsonPropertyName("suit")] string Suit,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("xp")] int Xp
);

public record SLOTC(
    [property: JsonPropertyName("attcol")] Attcol Attcol,
    [property: JsonPropertyName("crewweapon")] Crewweapon Crewweapon,
    [property: JsonPropertyName("eyecol")] Eyecol Eyecol,
    [property: JsonPropertyName("faction")] string Faction,
    [property: JsonPropertyName("pricol")] Pricol Pricol,
    [property: JsonPropertyName("role")] int Role,
    [property: JsonPropertyName("secondincommand")] bool Secondincommand,
    [property: JsonPropertyName("sigcol")] IReadOnlyList<object> Sigcol,
    [property: JsonPropertyName("skills")] Skills Skills,
    [property: JsonPropertyName("suit")] string Suit,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("xp")] int Xp
);

public record SlotLevels(
    [property: JsonPropertyName("0")] int _0,
    [property: JsonPropertyName("1")] int _1,
    [property: JsonPropertyName("10")] int _10,
    [property: JsonPropertyName("11")] int _11,
    [property: JsonPropertyName("12")] int _12,
    [property: JsonPropertyName("13")] int _13,
    [property: JsonPropertyName("14")] int _14,
    [property: JsonPropertyName("2")] int _2,
    [property: JsonPropertyName("3")] int _3,
    [property: JsonPropertyName("4")] int _4,
    [property: JsonPropertyName("5")] int _5,
    [property: JsonPropertyName("6")] int _6,
    [property: JsonPropertyName("7")] int _7,
    [property: JsonPropertyName("8")] int _8,
    [property: JsonPropertyName("9")] int _9
);

public record SURVIVABILITY(
    [property: JsonPropertyName("Assigned")] int Assigned
);

public record Upgrade(
    [property: JsonPropertyName("rank")] int Rank,
    [property: JsonPropertyName("uniqueName")] string UniqueName
);

public record Warframe(
    [property: JsonPropertyName("attcol")] Attcol Attcol,
    [property: JsonPropertyName("eyecol")] Eyecol Eyecol,
    [property: JsonPropertyName("features")] int Features,
    [property: JsonPropertyName("polarized")] int Polarized,
    [property: JsonPropertyName("pricol")] Pricol Pricol,
    [property: JsonPropertyName("sigcol")] Sigcol Sigcol,
    [property: JsonPropertyName("skins")] IReadOnlyList<Skin> Skins,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("upgrades")] IReadOnlyList<Upgrade> Upgrades,
    [property: JsonPropertyName("xp")] int Xp
);


#endregion
