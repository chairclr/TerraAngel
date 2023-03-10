using Microsoft.Xna.Framework.Input;

namespace TerraAngel.Tools.Developer;

public class NoClipTool : Tool
{
    public override string Name => "Noclip";

    public override ToolTabs Tab => ToolTabs.MainTools;

    public float NoClipSpeed = 20.8f;
    public int NoClipPlayerSyncTime = 1;

    public bool Enabled;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox(Name, ref Enabled);
        if (Enabled)
        {
            if (ImGui.CollapsingHeader("Noclip Settings"))
            {
                ImGui.Indent();
                ImGui.TextUnformatted("Speed"); ImGui.SameLine();
                ImGui.SliderFloat("##Speed", ref NoClipSpeed, 1f, 100f);

                ImGui.TextUnformatted("Frames between sync"); ImGui.SameLine();
                ImGui.SliderInt("##SyncTime", ref NoClipPlayerSyncTime, 1, 60);
                ImGui.Unindent();
            }
        }
    }

    public override void Update()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        if (!Main.mapFullscreen)
        {
            Player self = Main.LocalPlayer;

            if (InputSystem.IsKeyPressed(ClientConfig.Settings.ToggleNoclip))
            {
                Enabled = !Enabled;
            }

            if (!io.WantCaptureKeyboard && !io.WantTextInput && !Main.drawingPlayerChat)
            {
                if (Enabled)
                {
                    self.oldPosition = self.position;
                    if (io.KeysDown[(int)Keys.W])
                    {
                        self.position.Y -= NoClipSpeed;
                    }
                    if (io.KeysDown[(int)Keys.S])
                    {
                        self.position.Y += NoClipSpeed;
                    }
                    if (io.KeysDown[(int)Keys.A])
                    {
                        self.position.X -= NoClipSpeed;
                    }
                    if (io.KeysDown[(int)Keys.D])
                    {
                        self.position.X += NoClipSpeed;
                    }
                }
            }
        }
    }
}
