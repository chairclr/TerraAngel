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

        public ref Color TracerColor => ref ClientLoader.Config.TracerColor;

        public override void DrawUI(ImGuiIOPtr io)
        {
            base.DrawUI(io);

            if (ImGui.CollapsingHeader("ESP settings"))
            {
                ESPBoxesCringe espBoxes = CringeManager.GetCringe<ESPBoxesCringe>();
                ImGuiUtil.ColorEdit4("Local player box color", ref espBoxes.LocalPlayerColor);
                ImGuiUtil.ColorEdit4("Other player box color", ref espBoxes.OtherPlayerColor);
                ImGuiUtil.ColorEdit4("Other TerraAngel user box color", ref espBoxes.OtherTerraAngelUserColor);
                ImGuiUtil.ColorEdit4("NPC box color", ref espBoxes.NPCColor);
                ImGuiUtil.ColorEdit4("NPC net box color", ref espBoxes.NPCNetOffsetColor);
                ImGuiUtil.ColorEdit4("Projectile box color", ref espBoxes.ProjectileColor);
                ImGuiUtil.ColorEdit4("Tracer color", ref TracerColor);
            }
        }
    }
}
