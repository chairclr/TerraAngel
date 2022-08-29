namespace TerraAngel.Cheat.Cringes
{
    public class OptimizationCringe : Cringe
    {
        public override string Name => "Optimization Cringe";

        public override CringeTabs Tab => CringeTabs.VisualUtility;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultDisableDust))]
        public bool DisableDust;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultDisableGore))]
        public bool DisableGore;

        public override void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox("Disable Dust", ref DisableDust);
            ImGui.Checkbox("Disable Gore", ref DisableGore);
        }
    }
}
