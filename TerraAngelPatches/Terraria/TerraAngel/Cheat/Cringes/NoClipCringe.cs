using Microsoft.Xna.Framework.Input;

namespace TerraAngel.Cheat.Cringes
{
    public class NoClipCringe : Cringe
    {
        public override string Name => "Noclip";

        public override CringeTabs Tab => CringeTabs.MainCringes;

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
                    if (InputSystem.IsKeyPressed(ClientConfig.Settings.TeleportToCursor))
                    {
                        Main.LocalPlayer.velocity = Vector2.Zero;
                        Main.LocalPlayer.Teleport(Util.ScreenToWorldWorld(InputSystem.MousePosition) - new Vector2(Main.LocalPlayer.width / 2f, Main.LocalPlayer.height), TeleportationStyleID.RodOfDiscord);

                        NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);

                        if (ClientConfig.Settings.TeleportSendRODPacket)
                        {
                            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null,
                                0,
                                Main.LocalPlayer.whoAmI,
                                Main.LocalPlayer.position.X,
                                Main.LocalPlayer.position.Y,
                                TeleportationStyleID.RodOfDiscord);
                        }
                    }
                }


            }
            else
            {
                if (ClientConfig.Settings.RightClickOnMapToTeleport && (InputSystem.RightMousePressed || (io.KeyCtrl && InputSystem.RightMouseDown)) && !io.WantCaptureMouse)
                {
                    Main.LocalPlayer.velocity = Vector2.Zero;
                    if (io.KeyCtrl)
                    {
                        Main.LocalPlayer.Bottom = Util.ScreenToWorldFullscreenMap(InputSystem.MousePosition);
                    }
                    else
                    {
                        Main.mapFullscreen = false;
                        Main.LocalPlayer.Teleport(Util.ScreenToWorldFullscreenMap(InputSystem.MousePosition) - new Vector2(Main.LocalPlayer.width / 2f, Main.LocalPlayer.height), TeleportationStyleID.RodOfDiscord);
                        if (ClientConfig.Settings.TeleportSendRODPacket)
                        {
                            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null,
                                0,
                                Main.LocalPlayer.whoAmI,
                                Main.LocalPlayer.position.X,
                                Main.LocalPlayer.position.Y,
                                TeleportationStyleID.RodOfDiscord);
                        }
                    }

                    NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);
                }
            }
        }
    }
}
