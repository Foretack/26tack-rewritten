using Serilog;
using Tack.Modules;
using Tack.Nonclass;

namespace Tack.Handlers;
internal static class ModulesHandler
{
    private static readonly Dictionary<string, ChatModule> _chatModules = new();

    public static void Initialize()
    {
        AddChatModule(new LinkCollection());
    }

    public static bool EnableChatModule(string name)
    {
        if (!_chatModules.ContainsKey(name)) return false;

        _chatModules[name].Enable();
        return true;
    }

    public static bool DisableChatModule(string name)
    {
        if (!_chatModules.ContainsKey(name)) return false;

        _chatModules[name].Disable();
        return true;
    }

    private static void AddChatModule(ChatModule module)
    {
        _chatModules.Add(module.Name, module);
        Log.Verbose($"Loaded module: {module.Name}");
    }
}
