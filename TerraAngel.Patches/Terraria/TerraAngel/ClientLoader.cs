using TerraAngel.Plugins;

namespace TerraAngel;

internal static class ClientLoader
{
    public static readonly PluginService PluginService = new PluginService();

    public static void Load()
    {
        // Initial caching of plugins
        // For testing purposes for now
        PluginService.FindPlugins();
    }
}
