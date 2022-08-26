namespace TerraAngel.Cheat.Cringes
{
    public class AntiHurtCringe : Cringe
    {
        public override string Name => "Anti-hurt/Godmode";

        public override CringeTabs Tab => CringeTabs.MainCringes;

        [DefaultConfigValue("DefaultAntiHurt")]
        public bool Enabled;

        public int FramesSinceLastLifePacket = 0;

        public override void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox(Name, ref Enabled);
        }
    }
}
