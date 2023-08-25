using System;
using TerraAngel.Plugins;
using TerraAngel.UI;

namespace TerraAngel;

internal static class ClientLoader
{
    public static readonly PluginService PluginService = new PluginService();

    public static PluginUIState? PluginUIState { get; private set; }

    private static bool Loaded = false;

    private static bool Initialized = false;

    public static void Load()
    {
        if (Loaded)
        {
            throw new InvalidOperationException("Cannot load more than once");
        }

        Loaded = true;

        // Initial caching of plugins
        // For testing purposes for now
        PluginService.FindPlugins();
    }

    public static void Initialize()
    {
        if (Initialized)
        {
            throw new InvalidOperationException("Cannot initialize more than once");
        }

        Initialized = true;

        PluginUIState = new PluginUIState();
    }
}
