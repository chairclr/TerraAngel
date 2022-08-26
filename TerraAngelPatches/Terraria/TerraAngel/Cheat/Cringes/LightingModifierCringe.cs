namespace TerraAngel.Cheat.Cringes
{
    public class LightingModifierCringe : Cringe
    {
        public override string Name => "Lighting Modification";

        public override CringeTabs Tab => CringeTabs.LightingCringes;


        [DefaultConfigValue("PartialBrightDefaultValue")]
        public bool PartialBright;
        public ref float ExtraAirBrightness => ref ClientConfig.Settings.PartialBrightAirExtraLight;
        public ref float ExtraSolidBrightness => ref ClientConfig.Settings.PartialBrightSolidExtraLight;

        [DefaultConfigValue("FullBrightDefaultValue")]
        public bool FullBright;
        public ref float Brightness => ref ClientConfig.Settings.FullBrightBrightness;

        public override void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox("Full Bright", ref FullBright);

            ImGui.TextUnformatted("Brightness"); ImGui.SameLine();
            float tmp = Brightness * 100f;
            if (ImGui.SliderFloat("##Brightness", ref tmp, 1f, 100f))
            {
                Brightness = tmp / 100f;
            }

            ImGui.Checkbox("Partial-Bright", ref PartialBright);
            ImGui.TextUnformatted("Partial Air Brightness"); ImGui.SameLine();
            tmp = ExtraAirBrightness * 100f;
            if (ImGui.SliderFloat("##PartialAirBrightness", ref tmp, 1f, 100f))
            {
                ExtraAirBrightness = tmp / 100f;
            }
            ImGui.TextUnformatted("Partial Solid Brightness"); ImGui.SameLine();
            tmp = ExtraSolidBrightness * 100f;
            if (ImGui.SliderFloat("##PartialSolidBrightness", ref tmp, 1f, 100f))
            {
                ExtraSolidBrightness = tmp / 100f;
            }
        }

        public override void Update()
        {
            if (InputSystem.IsKeyPressed(ClientConfig.Settings.ToggleFullBright))
            {
                FullBright = !FullBright;
            }
        }
    }
}
