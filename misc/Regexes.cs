using System.Text.RegularExpressions;

namespace _26tack_rewritten.misc;

public static class Regexes
{
    public static readonly Regex Mention = new Regex(@"4s?tac?k|fo(re?[esk]|ur|r)tr?ac?k|129708505|\btest(ing)? ?(guy|individual)|login_unavailable|783267696|occluder", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    public static readonly Regex Racism = new Regex(@"(?:(?:\b(?<![-=\.])|monka)(?:[Nnñ]|[Ii7]V)|η|[\/|]\\[\/|])[\s\.]*?[liI1y!j\/|]+[\s\.]*?(?:[GgbB6934Q🅱qğĜƃ၅5\*][\s\.]*?){2,}(?!arcS|l|Ktlw|ylul|ie217|64|\d? ?times)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));
}
