using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;


namespace TerraAngel.Cheat.Cringes
{
    public class ESPBoxesCringe : Cringe
    {
        public override string Name => "ESP Boxes";
        public override CringeTabs Tab => CringeTabs.VisualUtility;

        public ref Color LocalPlayerColor => ref ClientLoader.Config.LocalBoxPlayerColor;
        public ref Color OtherPlayerColor => ref ClientLoader.Config.OtherBoxPlayerColor;
        public ref Color OtherTerraAngelUserColor => ref ClientLoader.Config.OtherTerraAngelUserColor;
        public ref Color NPCColor => ref ClientLoader.Config.NPCBoxColor;
        public ref Color NPCNetOffsetColor => ref ClientLoader.Config.NPCNetOffsetBoxColor;
        public ref Color ProjectileColor => ref ClientLoader.Config.ProjectileBoxColor;

        public bool NPCBoxes = false;
        public bool PlayerBoxes = false;
        public bool ProjectileBoxes = false;

        public override void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox("Player hitboxes", ref PlayerBoxes);
            ImGui.Checkbox("NPC hitboxes", ref NPCBoxes);
            ImGui.Checkbox("Projectile hitboxes", ref ProjectileBoxes);
        }
    }
}
