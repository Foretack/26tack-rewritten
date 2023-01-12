namespace Tack.Utils;
internal sealed class FixedStack<T> : Stack<T>
{
    private readonly int _maxSize;

    public FixedStack(int maxSize) : base(maxSize)
    {
        _maxSize = maxSize;
    }

    public new void Push(T item)
    {
        if (base.Count >= _maxSize) return;
        base.Push(item);
    }
}
