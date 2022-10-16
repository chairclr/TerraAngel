namespace TerraAngel.Cheat.Cringes;

public class InfiniteReachCringe : Cringe
{
    public override string Name => "Infinite reach";

    public override CringeTabs Tab => CringeTabs.MainCringes;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultInfiniteReach))]
    public bool Enabled;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
    }
}
