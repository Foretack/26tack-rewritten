using Serilog.Events;
using Tack.Core;
using Tack.Database;

namespace Tack.Models;
public sealed class ProgramSettings
{
    public LogEventLevel LogLevel { get; set; }
    public Dictionary<string, bool> EnabledModules { get; init; } = default!;

    public bool this[string moduleName]
    {
        get
        {
            if (EnabledModules.TryGetValue(moduleName, out bool enabled))
            {
                return enabled;
            }

            EnabledModules.Add(moduleName, true);
            return true;
        }
    }

    public Task UpdateCachedSettings()
    {
        Program.Settings.LogLevel = Program.LogSwitch.MinimumLevel;
        return Redis.Cache.SetObjectAsync("bot:settings", Program.Settings);
    }
}
