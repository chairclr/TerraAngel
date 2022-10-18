using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using TerraAngel.UI;
using Terraria.UI;

namespace TerraAngel.Plugin;

public class PluginLoader
{
    private static AssemblyLoadContext pluginLoader = new AssemblyLoadContext("PluginLoader", true);

    public static Dictionary<string, bool> AvailablePlugins = new Dictionary<string, bool>();

    public static List<Plugin> LoadedPlugins = new List<Plugin>();

    public static void LoadPlugins()
    {
        VerifyEnabled();
        foreach (string file in AvailablePlugins.Keys)
        {
            try
            {
                if (AvailablePlugins[file])
                {
                    using (FileStream sr = File.Open(file, FileMode.Open))
                    {
                        Assembly assembly = pluginLoader.LoadFromStream(sr);
                        LoadedPlugins.Add(LoadPluginFromDLL(assembly, file));
                        sr.Close();

                        ClientLoader.Console.WriteLine($"Loading {Path.GetFileName(file)}");
                    }
                }
            }
            catch (Exception ex)
            {
                ClientLoader.Console.WriteError(ex.ToString());
            }
        }

        InitPlugins();
    }
    public static void UnloadPlugins()
    {
        if (!LoadedPlugins.Any())
            return;
        DeinitPlugins();
        LoadedPlugins.Clear();
        pluginLoader.Unload();
        pluginLoader = new AssemblyLoadContext("PluginLoader", true);
        VerifyEnabled();
    }

    public static void VerifyEnabled()
    {
        Utility.Util.CreateDirectory(ClientLoader.PluginsPath);

        AvailablePlugins = AvailablePlugins.Where(x => File.Exists(x.Key)).ToDictionary(x => x.Key, x => x.Value);

        foreach (string file in Directory.GetFiles(ClientLoader.PluginsPath))
        {
            if (file.EndsWith(".TAPlugin.dll"))
            {
                if (!AvailablePlugins.ContainsKey(file))
                {
                    AvailablePlugins.Add(file, false);
                }
            }
        }
    }


    public static List<UIElement> GetPluginUIObjects()
    {
        List<UIElement> uiObjects = new List<UIElement>();

        VerifyEnabled();

        foreach (string file in AvailablePlugins.Keys)
        {
            uiObjects.Add(new UIPlugin(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file)), file, () => AvailablePlugins[file], (x) => AvailablePlugins[file] = x));
        }

        return uiObjects;
    }

    private static void InitPlugins()
    {
        foreach (Plugin? plugin in LoadedPlugins)
        {
            try
            {
                plugin?.Load();
            }
            catch (Exception ex)
            {
                ClientLoader.Console.WriteError($"Error loading {plugin.Name}/{plugin.PluginAssembly.FullName}: {ex}");
            }
        }
    }
    private static void DeinitPlugins()
    {
        foreach (Plugin? plugin in LoadedPlugins)
        {
            try
            {
                plugin?.Unload();
            }
            catch (Exception ex)
            {
                ClientLoader.Console.WriteError($"Error unloading {plugin.Name}/{plugin.PluginAssembly.FullName}: {ex}");
            }
        }
    }

    private static Plugin LoadPluginFromDLL(Assembly assembly, string path)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(Plugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                return (Plugin)Activator.CreateInstance(type, path);
            }
        }

        return null;
    }
}
