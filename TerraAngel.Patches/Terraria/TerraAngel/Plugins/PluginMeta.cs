using System.Xml.Serialization;

namespace TerraAngel.Plugins;

[XmlRoot(ElementName = "Plugin")]
public class PluginMeta
{
    [XmlAttribute(AttributeName = "name")]
    public string? Name { get; set; }

    [XmlAttribute(AttributeName = "author")]
    public string? Author { get; set; }

    [XmlAttribute(AttributeName = "description")]
    public string? Description { get; set; }

    [XmlAttribute(AttributeName = "entry_assembly_path")]
    public string? EntryAssemblyPath { get; set; }
}
