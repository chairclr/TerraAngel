using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace TerraAngel.Cheat.Cringes
{
    public class ProjectilePredictionCringe : Cringe
    {
        public override string Name => "Projectile Prediction";

        public override CringeTabs Tab => CringeTabs.VisualUtility;

        public float n = 0.2f;

        public override void DrawUI(ImGuiIOPtr io)
        {
            base.DrawUI(io);

            ImGui.SliderFloat("n", ref n, 0f, 1f);
        }
    }
}
