using Config.Net;
using Tack.Nonclass;

namespace Tack;
internal static class AppConfigLoader
{
    private static IAppConfig _appConfig = default!;
    private static IAppConfig Load()
    {
        return new ConfigurationBuilder<IAppConfig>().UseYamlFile("Config.yaml").Build();
    }

    public static IAppConfig AppConfig => _appConfig ??= Load();
    public static void ReloadConfig()
    {
        _appConfig = Load();
    }
}
