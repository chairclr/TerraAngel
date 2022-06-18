using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using TerraAngel.Loader;
using System.Runtime.Loader;

namespace TerraAngel.Plugin
{
    public class PluginLoader
    {
        private static AssemblyLoadContext loader = new AssemblyLoadContext("PluginLoader", true);

        public static List<IPlugin> LoadedPlugins = new List<IPlugin>();

        public static void LoadPlugins()
        {
            foreach (string file in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
            {
                if (file.EndsWith(".TAPlugin.dll"))
                {
                    using (FileStream sr = File.Open(file, FileMode.Open))
                    {
                        Assembly assembly = loader.LoadFromStream(sr);
                        LoadedPlugins.Add(LoadPluginFromDLL(assembly));
                    }
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
