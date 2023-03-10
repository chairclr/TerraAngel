namespace TerraAngel.Tools.Visuals;

public class HeldItemViewerTool : Tool
{
    public override string Name => "Show Held Item";

    public override ToolTabs Tab => ToolTabs.VisualTools;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultShowHeldItem))]
    public bool Enabled;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
    }
}
