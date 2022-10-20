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
        AddModule(new DiscordActivityNotifier());
    }

    private static void AddModule(IModule module)
    {
        _modules.Add(module.Name, module);
        Log.Verbose($"Loaded module: {module.Name}");
    }

    public static bool EnableModule(string name)
    {
        if (!_modules.ContainsKey(name)) return false;

        var module = _modules[name];
        if (module.Enabled) return true;

        module.Enable();
        return true;
    }

    public static bool DisableModule(string name)
    {
        if (!_modules.ContainsKey(name)) return false;

        var module = _modules[name];
        if (!module.Enabled) return true;

        _modules[name].Disable();
        return true;
    }
}
