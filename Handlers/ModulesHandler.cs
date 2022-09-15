using Serilog;
using Tack.Modules;
using Tack.Nonclass;

namespace Tack.Handlers;
internal static class ModulesHandler
{
    private static readonly Dictionary<string, IModule> _modules = new();

    public static void Initialize()
    {
        AddModule(new LinkCollection());
        AddModule(new MentionsRelay());
        AddModule(new BaroChecker());
    }

    private static void AddModule(IModule module)
    {
        _modules.Add(module.Name, module);
        Log.Verbose($"Loaded module: {module.Name}");
    }

    public static bool EnableModule(string name)
    {
        if (!_modules.ContainsKey(name)) return false;

        _modules[name].Enable();
        return true;
    }

    public static bool DisableModule(string name)
    {
        if (!_modules.ContainsKey(name)) return false;

        _modules[name].Disable();
        return true;
    }
}
