namespace TerraAngel.Tools.Visuals;

public class FreecamTool : Tool
{
    public override string Name => "Freecam";

    public override ToolTabs Tab => ToolTabs.VisualTools;

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
