using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using TerraAngel.Client.Config;
using Terraria;

namespace TerraAngel.Cheat.Cringes
{
    public class FullBrightCringe : Cringe
    {
        public override string Name => "Fullbright";

        public override CringeTabs Tab => CringeTabs.LightingCringes;

        public ref float Brightness => ref ClientConfig.Settings.FullBrightBrightness;

        [DefaultConfigValue("PartialBrightDefaultValue")]
        public bool PartialBright = false;
        public ref float ExtraAirBrightness => ref ClientConfig.Settings.PartialBrightAirExtraLight;
        public ref float ExtraSolidBrightness => ref ClientConfig.Settings.PartialBrightSolidExtraLight;

        public override void DrawUI(ImGuiIOPtr io)
        {
            base.DrawUI(io);

            ImGui.TextUnformatted("Brightness"); ImGui.SameLine();
            float tmp = Brightness * 100f;
            if (ImGui.SliderFloat("##Brightness", ref tmp, 1f, 100f))
            {
                Brightness = tmp / 100f;
                Main.renderNow = true;
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

        public override void OnEnable()
        {
            Main.renderNow = true;
        }
    }
}
