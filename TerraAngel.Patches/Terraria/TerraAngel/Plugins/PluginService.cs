using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;

namespace TerraAngel.Plugins;

internal class PluginService
{
    private PluginAssemblyService AssemblyService = new PluginAssemblyService();

    private XmlSerializer PluginMetaXmlSerializer = new XmlSerializer(typeof(PluginMeta));

    public PluginService()
    {

    }

    public void FindPlugins()
    {
        if (!Directory.Exists(PathService.PluginsRootFolder))
        {
            Directory.CreateDirectory(PathService.PluginsRootFolder);
        }

        foreach (string individualPluginFolder in Directory.EnumerateDirectories(PathService.PluginsRootFolder, "*", SearchOption.TopDirectoryOnly))
        {
            string potentialPluginMetadataPath = Path.Combine(individualPluginFolder, "plugin.xml");
            string pluginFolderName = Path.GetRelativePath(PathService.PluginsRootFolder, individualPluginFolder);

            // Look for PluginFolder/plugin.xml
            if (!File.Exists(potentialPluginMetadataPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No plugin.xml file for plugin in folder \"{pluginFolderName}\"");
                Console.ResetColor();
                continue;
            }

            // Try to parse the xml contained in PluginFolder/plugin.xml
            string text = File.ReadAllText(potentialPluginMetadataPath);

            if (!TryGetPluginMetadata(text, out PluginMeta? pluginMetadata))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"plugin.xml in folder \"{pluginFolderName}\" contains invalid, corrupt, or malformed xml.");
                Console.ResetColor();
                continue;
            }

            // It's valid to have no specified plugin name
            // It defaults to being the same name as the containing plugin folder
            if (pluginMetadata.Name is null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"plugin.xml in folder \"{pluginFolderName}\" does not specify a plugin name. Defaulting to containing folder name.");
                Console.ResetColor();

                pluginMetadata.Name = pluginFolderName;
            }

            // It's invalid to have no specified plugin entry assembly
            if (pluginMetadata.EntryAssemblyPath is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"plugin.xml in folder \"{pluginFolderName}\" does not specify a target entry assembly.");
                Console.ResetColor();
                continue;
            }

            // If the entry assembly path isn't fully qualified, fully qualify it relative to the plugin folder path
            if (!Path.IsPathFullyQualified(pluginMetadata.EntryAssemblyPath))
            {
                pluginMetadata.EntryAssemblyPath = Path.Combine(individualPluginFolder, pluginMetadata.EntryAssemblyPath);
            }

            if (!File.Exists(pluginMetadata.EntryAssemblyPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"plugin.xml in folder \"{pluginFolderName}\" specifies an invalid target entry assembly path.");
                Console.WriteLine($"The file \"{pluginMetadata.EntryAssemblyPath}\" does not exist.");
                Console.ResetColor();
                continue;
            }

            // Success! The plugin is valid and has a valid assembly path
        }
    }

    private bool TryGetPluginMetadata(string text, [NotNullWhen(true)] out PluginMeta? pluginMetadata)
    {
        using StringReader stringReader = new StringReader(text);
        pluginMetadata = null;

        try
        {
            pluginMetadata = (PluginMeta?)PluginMetaXmlSerializer.Deserialize(stringReader);
        }
        catch
        {

        }

        return pluginMetadata is not null;
    }
}
