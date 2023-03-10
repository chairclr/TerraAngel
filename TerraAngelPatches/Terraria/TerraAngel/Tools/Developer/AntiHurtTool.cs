namespace TerraAngel.Tools.Developer;

public class AntiHurtTool : Tool
{
    public override string Name => "Anti-hurt/Godmode";

    public override ToolTabs Tab => ToolTabs.MainTools;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultAntiHurt))]
    public bool Enabled;

    public int FramesSinceLastLifePacket = 0;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
    }
}
