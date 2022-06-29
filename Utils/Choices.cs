namespace Tack.Utils;

internal static class Choices
{
    private static readonly Random Rng = new Random();

    public static T Choice<T>(this IEnumerable<T> en) { return en.ElementAt(Rng.Next(en.Count())); }
    public static char Choice(this string str) { return str[Rng.Next(str.Length)]; }
}
