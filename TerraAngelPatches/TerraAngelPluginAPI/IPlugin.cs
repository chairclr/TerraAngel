using System.Reflection;

namespace TerraAngel.Plugin
{
    public abstract class Plugin
    {
        public abstract string Name { get; }

        public readonly Assembly PluginAssembly;

        public Plugin()
        {
            PluginAssembly = GetType().Assembly;
        }

        public virtual void Load()
        {

        }

        public virtual void Unload()
        {

        }

        public virtual void Update()
        {

        }
    }
}