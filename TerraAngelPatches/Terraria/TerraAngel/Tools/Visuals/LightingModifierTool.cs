namespace TerraAngel.Tools.Visuals;

public class LightingModifierTool : Tool
{
    public static LightingModifierTool? LightingModifierToolCache { get; private set; }

    public override string Name => "Lighting Modification";

    public override ToolTabs Tab => ToolTabs.LightingTools;

    [DefaultConfigValue(nameof(ClientConfig.Config.FullBrightDefaultValue))]
    public bool FullBright;

    public ref float Brightness => ref ClientConfig.Settings.FullBrightBrightness;

    public override void DrawUI(ImGuiIOPtr io)
    {
        if (ImGui.Checkbox("Full Bright", ref FullBright))
        {
            Lighting.Mode = Lighting.Mode;
        }

        ImGui.TextUnformatted("Brightness"); ImGui.SameLine();
        float tmp = Brightness * 100f;
        if (ImGui.SliderFloat("##Brightness", ref tmp, 1f, 100f))
        {
            Brightness = tmp / 100f;
        }
    }

    public override void Update()
    {
        if (InputSystem.IsKeyPressed(ClientConfig.Settings.ToggleFullBright))
        {
            FullBright = !FullBright;
            Lighting.Mode = Lighting.Mode;
        }

        LightingModifierToolCache = this;
        FullbrightEngine.Brightness = Brightness;
    }
}
