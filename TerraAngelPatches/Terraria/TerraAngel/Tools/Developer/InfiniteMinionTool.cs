namespace TerraAngel.Tools.Developer;

public class InfiniteMinionTool : Tool
{
    public override string Name => "Infinite minions";

    public override ToolTabs Tab => ToolTabs.MainTools;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultInfiniteMinions))]
    public bool Enabled;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
    }
}
