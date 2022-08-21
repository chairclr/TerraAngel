using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using TerraAngel.Cheat;
using TerraAngel.Cheat.Cringes;
using Terraria;
using TerraAngel.Utility;
using TerraAngel.Graphics;
using TerraAngel.WorldEdits;
using Terraria.ID;
using TerraAngel;
using NVector2 = System.Numerics.Vector2;
using TerraAngel.Client.Config;
using Microsoft.Xna.Framework.Input;
using Terraria.Map;
using TerraAngel.Input;
using TerraAngel.Hooks.Hooks;

namespace TerraAngel.Client.ClientWindows
{
    public class PlayerInspectorWindow : ClientWindow
    {
        public override string Title => "Player Inspector";
        public override bool DefaultEnabled => false;
        public override bool IsToggleable => true;
        public override bool IsPartOfGlobalUI => false;
        public override Keys ToggleKey => ClientConfig.Settings.ShowPlayerInspector;


        private int selectedPlayer = -1;

        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.PushFont(ClientAssets.GetMonospaceFont(16));
            bool notClickedClose = true;
            ImGui.Begin(Title, ref notClickedClose, ImGuiWindowFlags.MenuBar);

            bool showToolTip = true;
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Other Players"))
                {
                    for (int i = 0; i < 255; i++)
                    {
                        bool anyActivePlayers = false;
                        for (int j = i; j < Math.Min(i + 20, 255); j++)
                        {
                            if (Main.player[j].active)
                                anyActivePlayers = true;
                        }

                        bool endedDisableEarly = false;
                        ImGui.BeginDisabled(!anyActivePlayers);
                        if (ImGui.BeginMenu($"Players {i}-{Math.Min(i + 20, 255)}"))
                        {
                            endedDisableEarly = true;
                            showToolTip = false;
                            ImGui.EndDisabled();
                            for (int j = i; j < Math.Min(i + 20, 255); j++)
                            {
                                if (!Main.player[j].active) ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.Text] * new System.Numerics.Vector4(1f, 1f, 1f, 0.4f));
                                if (ImGui.MenuItem($"Player \"{Main.player[j].name.Truncate(30)}\""))
                                {
                                    selectedPlayer = j;
                                }
                                if (!Main.player[j].active) ImGui.PopStyleColor();
                            }
                            ImGui.EndMenu();
                        }
                        if (!endedDisableEarly)
                            ImGui.EndDisabled();
                        i += 20;
                    }
                    ImGui.EndMenu();
                }

                if (selectedPlayer > -1)
                {
                    Player player = Main.player[selectedPlayer];
                    if (ImGui.Button($"{Icon.Move}"))
                    {
                        Main.LocalPlayer.velocity = Vector2.Zero;
                        Main.LocalPlayer.Teleport(player.position, TeleportationStyleID.RodOfDiscord);

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
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"Teleport to \"{player.name.Truncate(30)}\"");
                        ImGui.EndTooltip();
                    }

                    if (ImGui.Button($"{Icon.CircleSlash}"))
                    {
                        Butcher.ButcherPlayer(player, CringeManager.ButcherDamage);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"Kill \"{player.name.Truncate(30)}\"");
                        ImGui.EndTooltip();
                    }

                    if (ImGui.Button($"{Icon.ScreenFull}"))
                    {
                        DrawHooks.SpectateOverride = player.whoAmI;
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"Spectate \"{player.name.Truncate(30)}\"");
                        ImGui.EndTooltip();
                    }
                }
                ImGui.EndMenuBar();
            }

            if (!notClickedClose)
            {
                IsEnabled = false;
                ImGui.End();
                return;
            }



            if (selectedPlayer > -1)
            {
                Player player = Main.player[selectedPlayer];
                ImGui.Text($"Inspecting player \"{player.name.Truncate(30)}\"");
                ImGui.Text($"HP:       {player.statLife.ToString().PadLeft(5),-7}/{player.statLifeMax2.ToString(),5}");
                ImGui.Text($"Mana:     {player.statMana.ToString().PadLeft(5),-7}/{player.statManaMax2.ToString(),5}");
                ImGui.Text($"Def:      {player.statDefense,5}");
                ImGui.Text($"Velocity: {MathF.Round(CalcSpeedMPH(player), 2),5:F1} mph");

                if (ImGui.CollapsingHeader("Player Inventory"))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float sy = ImGui.GetCursorPos().Y;
                    int c = 0;
                    for (int i = 0; i < 50; i++)
                    {
                        ImGuiUtil.ItemButton(player.inventory[i], $"pii{i}", new Vector2(24, 24), out _, out _, out _, showToolTip, false, player.inventory[i].stack == 1 ? 0 : player.inventory[i].stack, player.selectedItem == i);
                        
                        if ((i + 1) % 10 != 0)
                        {
                            ImGui.SameLine();
                        }
                        else
                        {
                            
                            if (i > 10)
                            {
                                if (c < 4)
                                {
                                    ImGui.SameLine();
                                    ImGuiUtil.ItemButton(player.inventory[50 + c], $"pii{i}", new Vector2(24, 24), out _, out _, out _, showToolTip, false, player.inventory[50 + c].stack == 1 ? 0 : player.inventory[50 + c].stack, player.selectedItem == 50 + c);
                                    ImGui.SameLine();
                                    ImGuiUtil.ItemButton(player.inventory[54 + c], $"pii{i}", new Vector2(24, 24), out _, out _, out _, showToolTip, false, player.inventory[54 + c].stack == 1 ? 0 : player.inventory[54 + c].stack, player.selectedItem == 54 + c);
                                }
                                c++;
                            }
                        }

                        if (i == 9)
                        {
                            ImGui.SameLine();
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 24 + style.ItemSpacing.X * 2);
                            ImGuiUtil.ItemButton(player.inventory[58], $"piim{58}", new Vector2(24, 24), out _, out _, out _, showToolTip, false, player.inventory[58].stack == 1 ? 0 : player.inventory[58].stack, false);
                        }
                    }

                    NVector2 lastCursorPos = ImGui.GetCursorPos();

                    float minx = MathF.Max((24f + style.ItemSpacing.X * 2f) * 12f + style.ItemSpacing.X, (ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X) - ((24f + style.ItemSpacing.X) * 3f + style.ItemSpacing.X));
                    int ti = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        if (i == 8 && !player.CanDemonHeartAccessoryBeShown()) continue;
                        if (i == 9 && !player.CanMasterModeAccessoryBeShown()) continue;


                        ImGui.SetCursorPos(new NVector2(minx, sy + (24f + style.ItemSpacing.X) * ti));

                        ImGuiUtil.ItemButton(player.dye[i], $"piid{i}", new Vector2(24, 24), out _, out _, out _, showToolTip, false, player.dye[i].stack == 1 ? 0 : player.dye[i].stack, false);
                        ImGui.SameLine();
                        ImGuiUtil.ItemButton(player.armor[i + 10], $"piia{i + 10}", new Vector2(24, 24), out _, out _, out _, showToolTip, false, player.armor[i + 10].stack == 1 ? 0 : player.armor[i + 10].stack, false);
                        ImGui.SameLine();
                        ImGuiUtil.ItemButton(player.armor[i], $"piia{i}", new Vector2(24, 24), out _, out _, out _, showToolTip, false, player.armor[i].stack == 1 ? 0 : player.armor[i].stack, false);

                        ti++;
                    }

                    ImGui.SetCursorPos(lastCursorPos);
                }
            }

            ImGui.End();
            ImGui.PopFont();
        }

        public override void Update()
        {
            if (!Main.mapFullscreen)
            {
                if (ClientConfig.Settings.RightClickOnPlayerToInspect)
                {
                    for (int i = 0; i < 255; i++)
                    {
                        Player player = Main.player[i];
                        if (player.active && player.whoAmI != Main.myPlayer)
                        {
                            if (Input.InputSystem.RightMousePressed && player.getRect().Contains(Util.ScreenToWorld(Input.InputSystem.MousePosition).ToPoint()))
                            {
                                selectedPlayer = player.whoAmI;
                                IsEnabled = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private float CalcSpeedMPH(Player player)
        {
            Vector2 correctedVelocity = player.velocity + player.instantMovementAccumulatedThisFrame;
            if (player.mount.Active && player.mount.IsConsideredASlimeMount && player.velocity.Y != 0f && !player.SlimeDontHyperJump)
            {
                correctedVelocity.Y += player.velocity.Y;
            }
            int speedSliceLen = (int)(1f + correctedVelocity.Length() * 6f);
            if (speedSliceLen > player.speedSlice.Length)
            {
                speedSliceLen = player.speedSlice.Length;
            }

            float num16 = 0f;
            for (int num17 = speedSliceLen - 1; num17 > 0; num17--)
            {
                player.speedSlice[num17] = player.speedSlice[num17 - 1];
            }

            player.speedSlice[0] = correctedVelocity.Length();
            for (int m = 0; m < player.speedSlice.Length; m++)
            {
                if (m < speedSliceLen)
                {
                    num16 += player.speedSlice[m];
                }
                else
                {
                    player.speedSlice[m] = num16 / (float)speedSliceLen;
                }
            }

            num16 /= (float)speedSliceLen;
            int num18 = 42240;
            int num19 = 216000;
            float playerSpeed = num16 * (float)num19 / (float)num18;
            if (!player.merman && !player.ignoreWater)
            {
                if (player.honeyWet)
                {
                    playerSpeed /= 4f;
                }
                else if (player.wet)
                {
                    playerSpeed /= 2f;
                }
            }

            return playerSpeed;
        }
    }
}
