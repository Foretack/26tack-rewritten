namespace Tack.Utils;
internal sealed class FixedStack<T> : Stack<T>
{
    public bool IsFull => base.Count >= _maxSize;

    private readonly int _maxSize;

    public FixedStack(int maxSize) : base(maxSize + 1)
    {
        _maxSize = maxSize;
    }

    public new void Push(T item)
    {
        if (IsFull) return;
        base.Push(item);
    }
}
