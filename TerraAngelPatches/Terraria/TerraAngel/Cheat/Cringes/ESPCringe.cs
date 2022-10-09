namespace TerraAngel.Cheat.Cringes
{
    public class ESPCringe : Cringe
    {
        public override string Name => "ESP Boxes";
        public override CringeTabs Tab => CringeTabs.ESP;

        public ref Color LocalPlayerColor => ref ClientConfig.Settings.LocalBoxPlayerColor;
        public ref Color OtherPlayerColor => ref ClientConfig.Settings.OtherBoxPlayerColor;
        public ref Color OtherTerraAngelUserColor => ref ClientConfig.Settings.OtherTerraAngelUserColor;
        public ref Color NPCColor => ref ClientConfig.Settings.NPCBoxColor;
        public ref Color NPCNetOffsetColor => ref ClientConfig.Settings.NPCNetOffsetBoxColor;
        public ref Color ProjectileColor => ref ClientConfig.Settings.ProjectileBoxColor;
        public ref Color ItemColor => ref ClientConfig.Settings.ItemBoxColor;
        public ref Color TracerColor => ref ClientConfig.Settings.TracerColor;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultDrawAnyESP))]
        public bool DrawAnyESP = true;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultMapESP))]
        public bool MapESP = true;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultPlayerESPBoxes))]
        public bool PlayerBoxes = false;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultPlayerESPTracers))]
        public bool PlayerTracers = false;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultNPCBoxes))]
        public bool NPCBoxes = false;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultProjectileBoxes))]
        public bool ProjectileBoxes = false;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultItemBoxes))]
        public bool ItemBoxes = false;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultTileSections))]
        public bool ShowTileSections = false;

        public override void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox("Draw Any ESP", ref DrawAnyESP);
            if (DrawAnyESP)
            {
                ImGui.Checkbox("Draw ESP on map", ref MapESP);
                ImGui.Checkbox("Player hitboxes", ref PlayerBoxes);
                ImGui.Checkbox("NPC hitboxes", ref NPCBoxes);
                ImGui.Checkbox("Projectile hitboxes", ref ProjectileBoxes);
                ImGui.Checkbox("Item hitboxes", ref ItemBoxes);
                ImGui.Checkbox("Player tracers", ref PlayerTracers);
                ImGui.Checkbox("Tile Sections", ref ShowTileSections);
                if (ImGui.CollapsingHeader("ESP Colors"))
                {
                    ImGui.Indent();
                    ImGuiUtil.ColorEdit4("Local player box color", ref LocalPlayerColor);
                    ImGuiUtil.ColorEdit4("Other player box color", ref OtherPlayerColor);
                    ImGuiUtil.ColorEdit4("Other TerraAngel user box color", ref OtherTerraAngelUserColor);
                    ImGuiUtil.ColorEdit4("Player Tracer color", ref TracerColor);
                    ImGuiUtil.ColorEdit4("NPC box color", ref NPCColor);
                    ImGuiUtil.ColorEdit4("NPC net box color", ref NPCNetOffsetColor);
                    ImGuiUtil.ColorEdit4("Projectile box color", ref ProjectileColor);
                    ImGuiUtil.ColorEdit4("Item box color", ref ItemColor);
                    ImGui.Unindent();
                }
            }
        }

        public override void Update()
        {
            if (InputSystem.IsKeyPressed(ClientConfig.Settings.ToggleDrawAnyESP))
                DrawAnyESP = !DrawAnyESP;
        }
    }
}
