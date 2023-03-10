namespace TerraAngel.Tools.Developer;

public class InfiniteReachTool : Tool
{
    public override string Name => "Infinite reach";

    public override ToolTabs Tab => ToolTabs.MainTools;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultInfiniteReach))]
    public bool Enabled;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
    }
}
