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
using TerraAngel.Utility;
using TerraAngel.Input;
using TerraAngel.Client.Config;

namespace TerraAngel.Cheat.Cringes
{
    public class NoClipCringe : Cringe
    {
        public override string Name => "Noclip";

        public override CringeTabs Tab => CringeTabs.MainCringes;

        public float NoClipSpeed = 20.8f;
        public int NoClipPlayerSyncTime = 1;

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

                if (Input.InputSystem.IsKeyPressed(ClientConfig.Settings.ToggleNoclip))
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
                        Main.LocalPlayer.Teleport(Util.ScreenToWorld(InputSystem.MousePosition) - new Vector2(Main.LocalPlayer.width / 2f, Main.LocalPlayer.height), TeleportationStyleID.RodOfDiscord);

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
                if (ClientConfig.Settings.RightClickOnMapToTeleport && (Input.InputSystem.RightMousePressed || (io.KeyCtrl && Input.InputSystem.RightMouseDown)) && !io.WantCaptureMouse)
                {
                    Main.LocalPlayer.velocity = Vector2.Zero;
                    if (io.KeyCtrl)
                    {
                        Main.LocalPlayer.Bottom = Util.ScreenToWorldFullscreenMap(Input.InputSystem.MousePosition);
                    }
                    else
                    {
                        Main.mapFullscreen = false;
                        Main.LocalPlayer.Teleport(Util.ScreenToWorldFullscreenMap(Input.InputSystem.MousePosition) - new Vector2(Main.LocalPlayer.width / 2f, Main.LocalPlayer.height), TeleportationStyleID.RodOfDiscord);
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
