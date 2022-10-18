namespace TerraAngel.Cheat.Cringes;

public class InfiniteManaCringe : Cringe
{
    public override string Name => "Infinite mana";

    public override CringeTabs Tab => CringeTabs.MainCringes;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultInfiniteMana))]
    public bool Enabled;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
    }
}
