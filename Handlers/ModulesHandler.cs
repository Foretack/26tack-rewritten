using Tack.Core;
using Tack.Modules;
using Tack.Nonclass;

namespace Tack.Handlers;
internal static class ModulesHandler
{
    private static readonly Dictionary<string, IModule> _modules = new();

    public static void Initialize()
    {
        AddModule(new LinkCollection(Program.Settings[nameof(LinkCollection)]));
        AddModule(new MentionsRelay(Program.Settings[nameof(MentionsRelay)]));
        AddModule(new BaroChecker(Program.Settings[nameof(BaroChecker)]));
        AddModule(new DiscordActivityNotifier(Program.Settings[nameof(DiscordActivityNotifier)]));
        AddModule(new Tf2NewsPrinter(Program.Settings[nameof(Tf2NewsPrinter)]));
        AddModule(new Fish(Program.Settings[nameof(Fish)]));
        AddModule(new FeedsReader(Program.Settings[nameof(FeedsReader)]));
        AddModule(new UserCollection(Program.Settings[nameof(UserCollection)]));
    }

    private static void AddModule(IModule module)
    {
        _modules.Add(module.Name, module);
        Log.Verbose("Loaded module: {name}", module.Name);
    }

    public static bool EnableModule(string name)
    {
        if (!_modules.ContainsKey(name))
            return false;

        IModule module = _modules[name];
        if (module.Enabled)
            return true;

        module.Enable();
        return true;
    }

    public static bool DisableModule(string name)
    {
        if (!_modules.ContainsKey(name))
            return false;

        IModule module = _modules[name];
        if (!module.Enabled)
            return true;

        _modules[name].Disable();
        return true;
    }

    public static string ListEnabledModules()
    {
        return string.Join(';', _modules.Where(x => x.Value.Enabled).Select(x => x.Value.Name));
    }
}
