namespace TerraAngel.Tools.Visuals;

public class DisableFramingTool : Tool
{
    public override string Name => "Disable tile framing";

    public override ToolTabs Tab => ToolTabs.VisualTools;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultDisableTileFraming))]
    public bool Enabled;

    public static bool FramingDsiabledCache = false;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
    }

    public override void Update()
    {
        FramingDsiabledCache = Enabled;
    }
}