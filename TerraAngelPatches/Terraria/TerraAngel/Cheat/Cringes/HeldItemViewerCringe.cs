namespace TerraAngel.Cheat.Cringes;

public class HeldItemViewerCringe : Cringe
{
    public override string Name => "Show held item";

    public override CringeTabs Tab => CringeTabs.VisualUtility;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultShowHeldItem))]
    public bool Enabled;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
    }
}
