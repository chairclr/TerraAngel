using System;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Hooks;
using Terraria.Graphics;
using Terraria.ID;

namespace TerraAngel.UI.ClientWindows;

public class PlayerInspectorWindow : ClientWindow
{
    public override string Title => "Player Inspector";

    public override bool DefaultEnabled => false;

    public override bool IsToggleable => true;

    public override bool IsGlobalToggle => false;

    public override Keys ToggleKey => ClientConfig.Settings.ShowPlayerInspector;

    private int SelectedPlayer = -1;

    public override void Draw(ImGuiIOPtr io)
    {
        ImGui.PushFont(ClientAssets.GetMonospaceFont(16));
        bool notClickedClose = true;
        ImGui.Begin(Title, ref notClickedClose, ImGuiWindowFlags.MenuBar);

        bool showTooltip = true;
        if (ImGui.BeginMenuBar())
        {
            DrawPlayerSelectMenu(out showTooltip);

            if (SelectedPlayer > -1)
            {
                Player player = Main.player[SelectedPlayer];
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
                    Butcher.ButcherPlayer(player, ToolManager.GetTool<ButcherTool>().ButcherDamage);
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

        if (SelectedPlayer > -1)
        {
            Player player = Main.player[SelectedPlayer];
            ImGui.Text($"Inspecting player \"{player.name.Truncate(30)}\"");
            ImGui.Text($"Health:   {player.statLife.ToString().PadLeft(5),-7}/{player.statLifeMax2,5}");
            ImGui.Text($"Mana:     {player.statMana.ToString().PadLeft(5),-7}/{player.statManaMax2,5}");
            ImGui.Text($"Defense:  {player.statDefense,5}");
            ImGui.Text($"Velocity: {MathF.Round(CalcSpeedMPH(player), 2),5:F1} mph");

            if (ImGui.CollapsingHeader("Player Inventory"))
            {
                ImGuiStylePtr style = ImGui.GetStyle();
                float sy = ImGui.GetCursorPos().Y;
                int c = 0;

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4f));

                for (int i = 0; i < 50; i++)
                {
                    ImGuiUtil.ItemButton(player.inventory[i], $"pii{i}", new Vector2(26f), showTooltip, isSelected: player.selectedItem == i);

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
                                ImGuiUtil.ItemButton(player.inventory[50 + c], $"piic50{i}", new Vector2(26f), showTooltip, isSelected: player.selectedItem == 50 + c);
                                ImGui.SameLine();
                                ImGuiUtil.ItemButton(player.inventory[54 + c], $"piic54{i}", new Vector2(26f), showTooltip, isSelected: player.selectedItem == 54 + c);
                                
                            }

                            c++;
                        }
                    }

                    if (i == 9)
                    {
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 26f - style.ItemSpacing.X * 2);
                        ImGuiUtil.ItemButton(player.inventory[58], $"piim{58}", new Vector2(26f), showTooltip, isSelected: player.selectedItem == 58);
                    }
                }

                Vector2 cPosNL = ImGui.GetCursorPos();

                ImGui.SameLine();

                Vector2 cPos = ImGui.GetCursorPos();

                ImGui.NewLine();

                Vector2 lastCursorPos = ImGui.GetCursorPos();

                float minx = MathF.Max(cPos.X, (ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X) - ((26f + style.ItemSpacing.X) * 5f + style.ItemSpacing.X * 3f));
                int ti = 0;
                for (int i = 0; i < 10; i++)
                {
                    if (i == 8 && !player.CanDemonHeartAccessoryBeShown()) continue;
                    if (i == 9 && !player.CanMasterModeAccessoryBeShown()) continue;

                    ImGui.SetCursorPos(new Vector2(minx, sy + (26f + style.ItemSpacing.X * 2.5f) * ti));

                    if (i < 10)
                    {
                        ImGuiUtil.ItemButton(player.dye[i], $"piid{i}", new Vector2(26f), showTooltip);
                        ImGui.SameLine();
                        ImGuiUtil.ItemButton(player.armor[i + 10], $"piia{i + 10}", new Vector2(26f), showTooltip);
                        ImGui.SameLine();
                        ImGuiUtil.ItemButton(player.armor[i], $"piia{i}", new Vector2(26f), showTooltip);
                        if (i < 5)
                        {
                            ImGui.SameLine();
                            ImGuiUtil.ItemButton(player.miscDyes[i], $"piiemd1{i}", new Vector2(26f), showTooltip);
                            ImGui.SameLine();
                            ImGuiUtil.ItemButton(player.miscEquips[i], $"piiem1{i}", new Vector2(26f), showTooltip);
                        }
                    }

                    ti++;
                }

                ImGui.SetCursorPos(cPosNL + new Vector2(11f * 26f + style.ItemSpacing.X * 9f + 2f, 0f));

                ImGuiUtil.ItemButton(player.trashItem, $"piit0", new Vector2(26f), showTooltip);

                ImGui.PopStyleVar();

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
                        if (InputSystem.RightMousePressed && player.getRect().Contains(Util.ScreenToWorldWorld(InputSystem.MousePosition).ToPoint()))
                        {
                            SelectedPlayer = player.whoAmI;
                            IsEnabled = true;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void DrawPlayerSelectMenu(out bool showTooltip)
    {
        showTooltip = true;

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
                    showTooltip = false;
                    ImGui.EndDisabled();
                    for (int j = i; j < Math.Min(i + 20, 255); j++)
                    {
                        if (!Main.player[j].active) ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.Text] * new Vector4(1f, 1f, 1f, 0.4f));
                        if (ImGui.MenuItem($"Player \"{Main.player[j].name.Truncate(30)}\""))
                        {
                            SelectedPlayer = j;
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
