using System.Text.RegularExpressions;

namespace _26tack_rewritten.misc;

public static class Regexes
{
    public static readonly Regex Mention = new Regex("4(s)?ta(c)?k|fo(r(e)?[esk]|ur|r)t[r]?a(c)?k|129708505|test(ing)?( )?(guy|individual)|occluder", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    public static readonly Regex Racism = new Regex(@"(?:(?:\b(?<![-=\.])|monka)(?:[Nnñ]|[Ii7]V)|η|[\/|]\\[\/|])[\s\.]*?[liI1y!j\/|]+[\s\.]*?(?:[GgbB6934Q🅱qğĜƃ၅5\*][\s\.]*?){2,}(?!arcS|l|Ktlw|ylul|ie217|64|\d? ?times)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));
}
