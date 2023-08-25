using System;

namespace TerraAngel.Plugins;

/// <summary>
/// Annotates the plugin class to be used as the entry point for the plugin when loading.
/// Only one of these may exist per plugin.
/// The plugin class must be public.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class PluginEntryAttribute : Attribute
{
    public PluginEntryAttribute()
    {

    }
}
