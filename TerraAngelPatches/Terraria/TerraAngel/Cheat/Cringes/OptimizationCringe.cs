using TerraAngel.Hooks;

namespace TerraAngel.Cheat.Cringes;

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

    public override void Update()
    {
        DrawHooks.OptimizationCache = this;
        Dust.OptimizationCache = this;
        Dust.DustIntersectionRectangle = new Rectangle((int)Main.screenPosition.X - 50, (int)Main.screenPosition.Y - 50, Main.screenWidth + 50, Main.screenHeight + 50);
    }
}
