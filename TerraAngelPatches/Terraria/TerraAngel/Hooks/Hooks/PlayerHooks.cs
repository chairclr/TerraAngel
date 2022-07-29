using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using Terraria;
using TerraAngel;
using ImGuiNET;
using ReLogic.OS;
using Terraria.DataStructures;
using Terraria.ID;
using TerraAngel.Cheat;
using TerraAngel.Cheat.Cringes;
using TerraAngel.Hooks;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Net;
using TerraAngel.Client.Config;

namespace TerraAngel.Hooks.Hooks
{
    public class PlayerHooks
    {
        public static void Generate()
        {
            HookUtil.HookGen<Player>("Hurt", PlayerHurtHook);
            HookUtil.HookGen<Player>("KillMe", PlayerKillHook);
            HookUtil.HookGen<Player>("ResetEffects", PlayerResetEffectsHook);
        }

        public static double PlayerHurtHook(Func<Player, PlayerDeathReason, int, int, bool, bool, bool, int, double> orig, Player self, PlayerDeathReason damageSource, int Damage, int hitDirection, bool pvp, bool quiet, bool Crit, int cooldownCounter)
        {
            if (self.whoAmI == Main.myPlayer && CringeManager.GetCringe<AntiHurtCringe>().Enabled)
            {
                if (CringeManager.GetCringe<AntiHurtCringe>().FramesSinceLastLifePacket == 0)
                {
                    CringeManager.GetCringe<AntiHurtCringe>().FramesSinceLastLifePacket = 6;
                    NetMessage.SendData(MessageID.PlayerLife, -1, -1, null, self.whoAmI);
                }
                return 0.0d;
            }

            return orig(self, damageSource, Damage, hitDirection, pvp, quiet, Crit, cooldownCounter);
        }
        public static void PlayerKillHook(Action<Player, PlayerDeathReason, double, int, bool> orig, Player self, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp)
        {
            if (self.whoAmI == Main.myPlayer && CringeManager.GetCringe<AntiHurtCringe>().Enabled)
            {
                NetMessage.SendData(MessageID.PlayerLife, -1, -1, null, self.whoAmI);
                return;
            }
            orig(self, damageSource, dmg, hitDirection, pvp);
        }
        public static void PlayerResetEffectsHook(Action<Player> orig, Player self)
        {
            orig(self);

            if (ClientLoader.SetupRenderer && self.whoAmI == Main.myPlayer)
            {
                ImGuiIOPtr io = ImGui.GetIO();

                if (CringeManager.GetCringe<AntiHurtCringe>().Enabled)
                {
                    self.statLife = self.statLifeMax2;
                }

                if (CringeManager.GetCringe<InfiniteMinionCringe>().Enabled)
                {
                    self.maxMinions = int.MaxValue - 100000;
                }

                if (CringeManager.GetCringe<InfiniteManaCringe>().Enabled)
                {
                    self.statMana = self.statLifeMax2;
                    self.manaCost = 0.0f;
                }

                if (CringeManager.GetCringe<InfiniteReachCringe>().Enabled)
                {
                    Player.tileRangeX = 100;
                    Player.tileRangeY = 100;
                }
                else
                {
                    Player.tileRangeX = 5;
                    Player.tileRangeY = 4;
                }

                if (CringeManager.GetCringe<AntiHurtCringe>().FramesSinceLastLifePacket > 0)
                    CringeManager.GetCringe<AntiHurtCringe>().FramesSinceLastLifePacket--;



                if (Main.mapFullscreen)
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
                else
                {
                    NoClipCringe noClip = CringeManager.GetCringe<NoClipCringe>();
                    if (Input.InputSystem.IsKeyPressed(ClientLoader.Config.ToggleNoclip))
                    {
                        noClip.Enabled = !noClip.Enabled;
                    }

                    if (noClip.Enabled)
                    {
                        if (!io.WantCaptureKeyboard && !io.WantTextInput && !Main.drawingPlayerChat)
                        {
                            self.oldPosition = self.position;
                            if (io.KeysDown[(int)Keys.W])
                            {
                                self.position.Y -= noClip.NoClipSpeed;
                            }
                            if (io.KeysDown[(int)Keys.S])
                            {
                                self.position.Y += noClip.NoClipSpeed;
                            }
                            if (io.KeysDown[(int)Keys.A])
                            {
                                self.position.X -= noClip.NoClipSpeed;
                            }
                            if (io.KeysDown[(int)Keys.D])
                            {
                                self.position.X += noClip.NoClipSpeed;
                            }
                        }

                        if (Main.GameUpdateCount % noClip.NoClipPlayerSyncTime == 0)
                        {
                            SpecialNetMessage.SendData(MessageID.PlayerControls, null, self.whoAmI, self.position.X, self.position.Y, (float)self.selectedItem);
                        }
                    }


                    if (Input.InputSystem.IsKeyPressed(ClientLoader.Config.TeleportToCursor) && !Main.drawingPlayerChat && !io.WantTextInput)
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

                if (CringeManager.AutoButcherHostileNPCs)
                {
                    Butcher.ButcherAllHostileNPCs(CringeManager.ButcherDamage);
                }

                if (CringeManager.NebulaSpam)
                {
                    for (int i = 0; i < CringeManager.NebulaSpamPower; i++)
                    {
                        NetMessage.SendData(102, -1, -1, null, Main.myPlayer, 173, Main.LocalPlayer.position.X, Main.LocalPlayer.position.Y);
                    }
                }
            }
        }
    }
}
