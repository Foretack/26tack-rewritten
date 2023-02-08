namespace Tack.Utils;

internal static class Choices
{
    public static T Choice<T>(this IEnumerable<T> en) => en.ElementAt(Random.Shared.Next(en.Count()));
    public static char Choice(this string str) => str[Random.Shared.Next(str.Length)];
}