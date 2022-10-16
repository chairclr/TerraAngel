namespace TerraAngel.Cheat.Cringes;

public class FreecamCringe : Cringe
{
    public override string Name => "Freecam";

    public override CringeTabs Tab => CringeTabs.VisualUtility;

    public bool Enabled;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
    }

    public override void Update()
    {
        if (InputSystem.IsKeyPressed(ClientConfig.Settings.ToggleFreecam))
        {
            Enabled = !Enabled;
        }
    }
}
