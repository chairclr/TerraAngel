using System.IO;

namespace TerraAngel.Plugins;

public abstract class Plugin
{
    /// <summary>
    /// The root directory containing the plugin
    /// </summary>
    public string PluginDirectory { get; }

    public Plugin()
    {
        string pluginAssemblyPath = GetType().Assembly.Location;

        // Just have PathService.PluginRootFolder as a sanity check
        if (File.Exists(pluginAssemblyPath))
        {
            PluginDirectory = Path.GetDirectoryName(pluginAssemblyPath) ?? PathService.PluginRootFolder;
        }
        else
        {
            PluginDirectory = PathService.PluginRootFolder;
        }
    }

    /// <summary>
    /// Called before launching the game, before any loading happens
    /// </summary>
    public virtual void PreLaunch() { }

    /// <summary>
    /// Called during initial game loading
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Called every tick
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Called every tick the player is in a world
    /// </summary>
    public virtual void UpdateInWorld() { }

    /// <summary>
    /// Called every tick the player is in the main menu
    /// </summary>
    public virtual void UpdateInMainMenu() { }

    /// <summary>
    /// Called every frame
    /// </summary>
    public virtual void Draw() { }

    /// <summary>
    /// Called every frame the player is in a world
    /// </summary>
    public virtual void DrawInWorld() { }

    /// <summary>
    /// Called every frame the player is in the main menu
    /// </summary>
    public virtual void DrawInMainMenu() { }
}
