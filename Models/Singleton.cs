using System.Data;

namespace Tack.Models;
public abstract class Singleton
{
    private static readonly List<Type> _initialized = new();

    protected Singleton()
    {
        Type type = GetType();
        if (_initialized.Contains(type))
            throw new ConstraintException($"Cannot create another copy of Singleton class \"{type.Name}\"");

        _initialized.Add(type);
    }
}
