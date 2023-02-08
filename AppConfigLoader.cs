using Config.Net;
using Tack.Nonclass;

namespace Tack;
internal static class AppConfigLoader
{
    private static IAppConfig _appConfig;
    private static IAppConfig Load() => new ConfigurationBuilder<IAppConfig>().UseYamlFile("Config.yaml").Build();
    public static IAppConfig Config { get => _appConfig ??= Load(); }
    public static void ReloadConfig() => _appConfig = Load();
}
