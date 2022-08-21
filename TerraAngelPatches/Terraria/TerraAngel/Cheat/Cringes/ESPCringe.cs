using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using TerraAngel.Client.Config;
using TerraAngel.Graphics;

namespace TerraAngel.Cheat.Cringes
{
    public class ESPCringe : Cringe
    {
        public override string Name => "ESP Boxes";
        public override CringeTabs Tab => CringeTabs.VisualUtility;

        public ref Color LocalPlayerColor => ref ClientConfig.Settings.LocalBoxPlayerColor;
        public ref Color OtherPlayerColor => ref ClientConfig.Settings.OtherBoxPlayerColor;
        public ref Color OtherTerraAngelUserColor => ref ClientConfig.Settings.OtherTerraAngelUserColor;
        public ref Color NPCColor => ref ClientConfig.Settings.NPCBoxColor;
        public ref Color NPCNetOffsetColor => ref ClientConfig.Settings.NPCNetOffsetBoxColor;
        public ref Color ProjectileColor => ref ClientConfig.Settings.ProjectileBoxColor;
        public ref Color ItemColor => ref ClientConfig.Settings.ItemBoxColor;
        public ref Color TracerColor => ref ClientConfig.Settings.TracerColor;

        public bool DrawAnyESP = true;
        public bool PlayerBoxes = false;
        public bool PlayerTracers = false;
        public bool NPCBoxes = false;
        public bool ProjectileBoxes = false;
        public bool ItemBoxes = false;
        public bool ShowTileSections = false;

        public override void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox("Draw Any ESP", ref DrawAnyESP);
            if (DrawAnyESP)
            {
                ImGui.Checkbox("Player hitboxes", ref PlayerBoxes);
                ImGui.Checkbox("Player tracers", ref PlayerTracers);
                ImGui.Checkbox("NPC hitboxes", ref NPCBoxes);
                ImGui.Checkbox("Projectile hitboxes", ref ProjectileBoxes);
                ImGui.Checkbox("Item hitboxes", ref ItemBoxes);
                ImGui.Checkbox("Tile Sections", ref ShowTileSections);
                if (ImGui.CollapsingHeader("ESP settings"))
                {
                    ImGuiUtil.ColorEdit4("Local player box color", ref LocalPlayerColor);
                    ImGuiUtil.ColorEdit4("Other player box color", ref OtherPlayerColor);
                    ImGuiUtil.ColorEdit4("Other TerraAngel user box color", ref OtherTerraAngelUserColor);
                    ImGuiUtil.ColorEdit4("Player Tracer color", ref TracerColor);
                    ImGuiUtil.ColorEdit4("NPC box color", ref NPCColor);
                    ImGuiUtil.ColorEdit4("NPC net box color", ref NPCNetOffsetColor);
                    ImGuiUtil.ColorEdit4("Projectile box color", ref ProjectileColor);
                    ImGuiUtil.ColorEdit4("Item box color", ref ItemColor);
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (Input.InputSystem.IsKeyPressed(ClientConfig.Settings.ToggleDrawAnyESP))
                DrawAnyESP = !DrawAnyESP;
        }
    }
}
