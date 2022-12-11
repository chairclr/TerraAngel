using System.Reflection;

namespace TerraAngel.Plugin;

public abstract class Plugin
{
    public bool IsInited = false;

    public abstract string Name { get; }

    public readonly Assembly PluginAssembly;
    public readonly string PluginPath;

    public Plugin(string path)
    {
        PluginAssembly = GetType().Assembly;
        PluginPath = path;
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