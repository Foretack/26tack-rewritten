namespace Tack.Models;

internal static class SingleOf
{
    private static readonly Dictionary<Type, object> _singles = new();

    public static T Get<T>()
    {
        if (_singles.TryGetValue(typeof(T), out object? t))
            return (T)t;

        return default!;
    }

    public static void Set<T>(T value)
        where T : notnull
    {
        if (_singles.ContainsKey(typeof(T)))
            return;

        _singles[typeof(T)] = value;
    }

    public static bool HasType<T>()
    {
        return _singles.ContainsKey(typeof(T));
    }
}

internal static class SingleOf<T>
    where T : notnull
{
    public static T Obj { get; private set; } = SingleOf.HasType<T>() ? SingleOf.Get<T>() : throw new NullReferenceException($"No reference of {typeof(T)} exists");
}
