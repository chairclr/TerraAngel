using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using TerraAngel.Graphics;
using TerraAngel.Cheat;

namespace TerraAngel.Cheat.Cringes
{
    public class ESPTracersCringe : Cringe
    {
        public override string Name => "ESP tracers";

        public override CringeTabs Tab => CringeTabs.VisualUtility;

        public Color TracerColor = new Color(0f, 0f, 1f);

        public override void DrawUI(ImGuiIOPtr io)
        {
            base.DrawUI(io);

            if (ImGui.CollapsingHeader("ESP settings"))
            {
                ESPBoxesCringe espBoxes = CringeManager.GetCringe<ESPBoxesCringe>();
                ImGuiUtil.ColorEdit4("Local player box color", ref espBoxes.LocalPlayerColor);
                ImGuiUtil.ColorEdit4("Other player box color", ref espBoxes.OtherPlayerColor);
                ImGuiUtil.ColorEdit4("Tracer color", ref TracerColor);
            }
        }
    }
}
