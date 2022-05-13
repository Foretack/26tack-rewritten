namespace _26tack_rewritten.utils;

internal static class Random
{
    private static readonly System.Random Rng = new System.Random();

    public static T Choice<T>(this T[] array) { return array[Rng.Next(array.Length)]; }
    public static T Choice<T>(this List<T> list) {  return list[Rng.Next(list.Count)]; }
    public static char Choice(this string str) { return str[Rng.Next(str.Length)]; }
}
