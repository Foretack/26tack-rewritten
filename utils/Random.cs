namespace _26tack_rewritten.utils;

internal static class Random
{
    private static readonly System.Random Rng = new System.Random();

    public static T Choice<T>(params T[] itemsOrArray) { return itemsOrArray[Rng.Next(itemsOrArray.Length)]; }
    public static T Choice<T>(List<T> list) {  return list[Rng.Next(list.Count)]; }
    public static char Choice(string str) { return str[Rng.Next(str.Length)]; }
}
