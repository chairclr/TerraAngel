using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using TerraAngel.Loader;
using System.Runtime.Loader;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using TerraAngel.UI;

namespace TerraAngel.Plugin
{
    public class PluginLoader
    {
        private static AssemblyLoadContext loader = new AssemblyLoadContext("PluginLoader", true);

        public static Dictionary<string, bool> AvailablePlugins = new Dictionary<string, bool>();

        public static List<IPlugin> LoadedPlugins = new List<IPlugin>();

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
                            Assembly assembly = loader.LoadFromStream(sr);
                            LoadedPlugins.Add(LoadPluginFromDLL(assembly));
                            sr.Close();
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
            loader.Unload();
            loader = new AssemblyLoadContext("PluginLoader", true);
            VerifyEnabled();
        }

        public static void VerifyEnabled()
        {
            Utility.Util.CreateDirectory(ClientLoader.PluginsPath);

            List<string> toRemove = new List<string>();
            foreach (string file in AvailablePlugins.Keys)
            {
                if (!File.Exists(file))
                {
                    toRemove.Add(file);
                }
            }

            foreach (string file in toRemove)
            {
                AvailablePlugins.Remove(file);
            }

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
                uiObjects.Add(new UIPlugin(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file)), () => AvailablePlugins[file], (x) => AvailablePlugins[file] = x));
            }

            return uiObjects;
        }

        private static void InitPlugins()
        {
            foreach (IPlugin? plugin in LoadedPlugins)
            {
                plugin?.Load();
            }
        }
        private static void DeinitPlugins()
        {
            foreach (IPlugin? plugin in LoadedPlugins)
            {
                plugin?.Unload();
            }
        }


        
        private static IPlugin LoadPluginFromDLL(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface)
                {
                    return (IPlugin)Activator.CreateInstance(type);
                }
            }

            return null;
        }
    }
}
