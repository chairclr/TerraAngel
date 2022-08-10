using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace TerraAngel.Cheat.Cringes
{
    public class NoClipCringe : Cringe
    {
        public override string Name => "Noclip";

        public override CringeTabs Tab => CringeTabs.MainCringes;

        public float NoClipSpeed = 20.8f;
        public int NoClipPlayerSyncTime = 2;

        public override void DrawUI(ImGuiIOPtr io)
        {
            base.DrawUI(io);

            if (ImGui.CollapsingHeader("Noclip Settings"))
            {
                ImGui.TextUnformatted("Speed"); ImGui.SameLine();
                ImGui.SliderFloat("##Speed", ref NoClipSpeed, 1f, 100f);

                ImGui.TextUnformatted("Frames between sync"); ImGui.SameLine();
                ImGui.SliderInt("##SyncTime", ref NoClipPlayerSyncTime, 1, 60);
            }
        }

        public override void Update()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            if (!Main.mapFullscreen)
            {
                Player self = Main.LocalPlayer;

                if (Input.InputSystem.IsKeyPressed(ClientLoader.Config.ToggleNoclip))
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
                    if (Input.InputSystem.IsKeyPressed(ClientLoader.Config.TeleportToCursor))
                    {
                        Main.LocalPlayer.velocity = Vector2.Zero;
                        Main.LocalPlayer.Bottom = Utility.Util.ScreenToWorld(Input.InputSystem.MousePosition);
                        Main.LocalPlayer.Teleport(Main.LocalPlayer.position, TeleportationStyleID.RodOfDiscord);

                        NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);

                        if (ClientLoader.Config.TeleportSendRODPacket)
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
                if (ClientLoader.Config.RightClickOnMapToTeleport && (Input.InputSystem.RightMousePressed || (io.KeyCtrl && Input.InputSystem.RightMouseDown)) && !io.WantCaptureMouse)
                {
                    Main.LocalPlayer.velocity = Vector2.Zero;
                    Main.LocalPlayer.Bottom = Utility.Util.ScreenToWorldFullscreenMap(Input.InputSystem.MousePosition);
                    if (!io.KeyCtrl)
                        Main.LocalPlayer.Teleport(Main.LocalPlayer.position, TeleportationStyleID.RodOfDiscord);

                    if (!io.KeyCtrl)
                        if (ClientLoader.Config.TeleportSendRODPacket)
                        {
                            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null,
                                0,
                                Main.LocalPlayer.whoAmI,
                                Main.LocalPlayer.position.X,
                                Main.LocalPlayer.position.Y,
                                TeleportationStyleID.RodOfDiscord);
                        }

                    NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);

                    if (!io.KeyCtrl)
                        Main.mapFullscreen = false;
                }
            }
        }
    }
}
