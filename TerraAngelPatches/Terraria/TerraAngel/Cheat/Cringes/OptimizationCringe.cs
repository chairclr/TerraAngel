namespace TerraAngel.Cheat.Cringes
{
    public class OptimizationCringe : Cringe
    {
        public override string Name => "Optimization Cringe";

        public override CringeTabs Tab => CringeTabs.VisualUtility;

        [DefaultConfigValue("DefaultDisableDust")]
        public bool DisableDust;

        [DefaultConfigValue("DefaultDisableGore")]
        public bool DisableGore;

        public override void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox("Disable Dust", ref DisableDust);
            ImGui.Checkbox("Disable Gore", ref DisableGore);
        }
    }
}
