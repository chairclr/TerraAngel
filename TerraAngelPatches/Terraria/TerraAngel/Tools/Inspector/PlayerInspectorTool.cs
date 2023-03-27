using System;
using TerraAngel.Inspector.Tools;

namespace TerraAngel.Tools.Inspector;

public class PlayerInspectorTool : InspectorTool
{
    public override string Name => "Player Inspector";

    private bool ShowTooltip = true;

    private int SelectedPlayer = -1;

    public override void DrawMenuBar(ImGuiIOPtr io)
    {
        ShowTooltip = true;
        DrawPlayerSelectMenu(out ShowTooltip);

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
    }

    public override void DrawInspector(ImGuiIOPtr io)
    {
        if (SelectedPlayer <= -1)
        {
            return;
        }

        Player player = Main.player[SelectedPlayer];
        ImGui.Text($"Inspecting Player[{SelectedPlayer}] \"{player.name.Truncate(60)}\"");
        ImGui.Text($"Health:      {player.statLife}/{player.statLifeMax2}");
        ImGui.Text($"Mana:        {player.statMana}/{player.statManaMax2}");
        ImGui.Text($"Defense:     {player.statDefense}");
        ImGui.Text($"Speed:       {player.velocity.Length()}");
        ImGui.Text($"Velocity:    {player.velocity}");
        ImGui.Text($"Velocity Dir: ");

        if (player.velocity.Length() > 0f)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            Vector2 center = new Vector2(ImGui.GetItemRectMax().X, ImGui.GetItemRectMin().Y) + new Vector2(ImGui.GetItemRectSize().Y / 2f);
            Matrix3x2 rotationMatrix = Matrix3x2.CreateRotation(player.velocity.SafeNormalize(Vector2.Zero).AngleTo(Vector2.Zero) + MathF.PI / 2f);

            Vector2 head = center + Vector2.Transform(new Vector2(0f, ImGui.GetTextLineHeight() / 2f), rotationMatrix);
            Vector2 tail = center + Vector2.Transform(new Vector2(0f, -ImGui.GetTextLineHeight() / 2f), rotationMatrix);
            Vector2 tri1 = center + Vector2.Transform(new Vector2(0f, ImGui.GetTextLineHeight() / 2f), rotationMatrix);
            Vector2 tri2 = center + Vector2.Transform(new Vector2(ImGui.GetTextLineHeight() / 7f, ImGui.GetTextLineHeight() / 4f), rotationMatrix);
            Vector2 tri3 = center + Vector2.Transform(new Vector2(-ImGui.GetTextLineHeight() / 7f, ImGui.GetTextLineHeight() / 4f), rotationMatrix);

            drawList.AddLine(head, tail, Color.Red.PackedValue);
            drawList.AddTriangle(tri1, tri2, tri3, Color.Red.PackedValue);
            drawList.AddTriangle(tri1, tri2, tri3, Color.Red.PackedValue);
            drawList.AddTriangleFilled(tri1, tri2, tri3, Color.Red.PackedValue);
        }

        if (ImGui.CollapsingHeader("Player Inventory"))
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            float sy = ImGui.GetCursorPos().Y;
            int c = 0;

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4f));

            for (int i = 0; i < 50; i++)
            {
                ImGuiUtil.ItemButton(player.inventory[i], $"pii{i}", new Vector2(26f), ShowTooltip, isSelected: player.selectedItem == i);

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
                            ImGuiUtil.ItemButton(player.inventory[50 + c], $"piic50{i}", new Vector2(26f), ShowTooltip, isSelected: player.selectedItem == 50 + c);
                            ImGui.SameLine();
                            ImGuiUtil.ItemButton(player.inventory[54 + c], $"piic54{i}", new Vector2(26f), ShowTooltip, isSelected: player.selectedItem == 54 + c);

                        }

                        c++;
                    }
                }

                if (i == 9)
                {
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 26f - style.ItemSpacing.X * 2);
                    ImGuiUtil.ItemButton(player.inventory[58], $"piim{58}", new Vector2(26f), ShowTooltip, isSelected: player.selectedItem == 58);
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
                    ImGuiUtil.ItemButton(player.dye[i], $"piid{i}", new Vector2(26f), ShowTooltip);
                    ImGui.SameLine();
                    ImGuiUtil.ItemButton(player.armor[i + 10], $"piia{i + 10}", new Vector2(26f), ShowTooltip);
                    ImGui.SameLine();
                    ImGuiUtil.ItemButton(player.armor[i], $"piia{i}", new Vector2(26f), ShowTooltip);
                    if (i < 5)
                    {
                        ImGui.SameLine();
                        ImGuiUtil.ItemButton(player.miscDyes[i], $"piiemd1{i}", new Vector2(26f), ShowTooltip);
                        ImGui.SameLine();
                        ImGuiUtil.ItemButton(player.miscEquips[i], $"piiem1{i}", new Vector2(26f), ShowTooltip);
                    }
                }

                ti++;
            }

            ImGui.SetCursorPos(cPosNL + new Vector2(11f * 26f + style.ItemSpacing.X * 9f + 2f, 0f));

            ImGuiUtil.ItemButton(player.trashItem, $"piit0", new Vector2(26f), ShowTooltip);

            ImGui.PopStyleVar();

            ImGui.SetCursorPos(lastCursorPos);
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

    public override void UpdateInGameSelect()
    {
        for (int i = 0; i < 255; i++)
        {
            Player player = Main.player[i];
            if (player.active)
            {
                Rectangle selectRect = new Rectangle((int)(player.position.X + player.width * 0.5 - 16.0), (int)(player.position.Y + player.height - 48f), 32, 48);

                if (InputSystem.RightMousePressed && selectRect.Contains(Util.ScreenToWorldWorld(InputSystem.MousePosition).ToPoint()))
                {
                    SelectedPlayer = player.whoAmI;
                    InspectorWindow.OpenTab(this);
                    break;
                }
            }
        }
    }
}
