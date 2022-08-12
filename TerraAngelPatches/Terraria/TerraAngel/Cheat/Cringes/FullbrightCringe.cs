using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Terraria;

namespace TerraAngel.Cheat.Cringes
{
    public class FullBrightCringe : Cringe
    {
        public override string Name => "Fullbright";

        public override CringeTabs Tab => CringeTabs.LightingCringes;

        public ref float Brightness => ref ClientLoader.Config.FullBrightBrightness;

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
        }

        public override void OnEnable()
        {
            Main.renderNow = true;
        }
    }
}
