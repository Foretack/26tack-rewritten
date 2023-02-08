using System.Text.RegularExpressions;

namespace Tack.Models;
internal sealed class DiscordTrigger
{
    public ulong ChannelId { get; init; }
    public string NameContains { get; init; }
    public string RemoveText { get; init; }
    public string OutChannel { get; init; }
    public string PrependText { get; init; }
    public bool UseRegex { get; init; }
    public bool HasGroupReplacements { get; init; }
    public Regex ReplacementRegex { get; init; }
    public Dictionary<int, string> RegexGroupReplacements { get; } = new();

    public DiscordTrigger(dynamic x)
    {
        ChannelId = (ulong)x.channel_id;
        NameContains = x.name_contains;
        RemoveText = x.remove_text;
        OutChannel = x.output_channel;
        PrependText = x.prepend_text;
        UseRegex = x.use_regex;

        if (!UseRegex) return;
        ReplacementRegex = new Regex(x.replacement_regex);
        string replaceWith = x.replace_with;
        if (string.IsNullOrEmpty(replaceWith))
        {
            HasGroupReplacements = false;
            return;
        }

        var r = replaceWith.Split(";;").Select(x => x.Split("::"));
        foreach (var replacement in r)
        {
            if (int.TryParse(replacement[0], out int number))
                RegexGroupReplacements.Add(number, replacement[1] ?? string.Empty);
        }
        if (RegexGroupReplacements.Count > 0) HasGroupReplacements = true;
    }
}
