namespace TerraAngel.Cheat.Cringes;

public class DisableFramingCringe : Cringe
{
    public override string Name => "Disable tile framing";

    public override CringeTabs Tab => CringeTabs.VisualUtility;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultDisableTileFraming))]
    public bool Enabled;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
    }
}
