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

namespace TerraAngel.Client.ClientWindows
{
    public class PlayerInspectorWindow : ClientWindow
    {
        public override string Title => "Player Inspector";
        public override bool DefaultEnabled => false;
        public override bool IsEnabled => selectedPlayer > 0;
        public override bool IsToggleable => false;
        public override bool IsPartOfGlobalUI => false;


        private int selectedPlayer = -1;

        public override void Draw(ImGuiIOPtr io)
        {
            if (selectedPlayer > 0)
            {
                ImGui.PushFont(ClientAssets.GetMonospaceFont(16));
                bool notClickedClose = true;
                ImGui.Begin(Title, ref notClickedClose, ImGuiWindowFlags.NoSavedSettings);
                if (!notClickedClose)
                {
                    selectedPlayer = -1;
                    ImGui.End();
                    return;
                }

                Player player = Main.player[selectedPlayer];
                ImGui.Text($"Inspecting player \"{player.name}\"");


                if (ImGui.CollapsingHeader("Player Inventory"))
                {
                    ImGuiStylePtr style = ImGui.GetStyle();
                    float sy = ImGui.GetCursorPos().Y;
                    int c = 0;
                    for (int i = 0; i < 50; i++)
                    {
                        ImGuiUtil.ItemButton(player.inventory[i].type, $"pii{i}", new Vector2(24, 24), out _, out _, out _, true, false, player.inventory[i].stack == 1 ? 0 : player.inventory[i].stack, player.selectedItem == i);
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
                                    ImGuiUtil.ItemButton(player.inventory[50 + c].type, $"pii{i}", new Vector2(24, 24), out _, out _, out _, true, false, player.inventory[50 + c].stack == 1 ? 0 : player.inventory[50 + c].stack, player.selectedItem == 50 + c);
                                    ImGui.SameLine();
                                    ImGuiUtil.ItemButton(player.inventory[54 + c].type, $"pii{i}", new Vector2(24, 24), out _, out _, out _, true, false, player.inventory[54 + c].stack == 1 ? 0 : player.inventory[54 + c].stack, player.selectedItem == 54 + c);
                                }
                                c++;
                            }
                        }
                    }

                    float minx = MathF.Max((24f + style.ItemSpacing.X * 2f) * 12f + style.ItemSpacing.X, (ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X) - (24f + style.ItemSpacing.X * 2f) * 3f);
                    int ti = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        if (i == 8 && !player.CanDemonHeartAccessoryBeShown()) continue;
                        if (i == 9 && !player.CanMasterModeAccessoryBeShown()) continue;


                        ImGui.SetCursorPos(new NVector2(minx, sy + (24f + style.ItemSpacing.X) * ti));

                        ImGuiUtil.ItemButton(player.dye[i].type, $"piid{i}", new Vector2(24, 24), out _, out _, out _, true, false, player.dye[i].stack == 1 ? 0 : player.dye[i].stack, false);
                        ImGui.SameLine();
                        ImGuiUtil.ItemButton(player.armor[i + 10].type, $"piia{i + 10}", new Vector2(24, 24), out _, out _, out _, true, false, player.armor[i + 10].stack == 1 ? 0 : player.armor[i + 10].stack, false);
                        ImGui.SameLine();
                        ImGuiUtil.ItemButton(player.armor[i].type, $"piia{i}", new Vector2(24, 24), out _, out _, out _, true, false, player.armor[i].stack == 1 ? 0 : player.armor[i].stack, false);

                        ti++; 
                    }
                }

                ImGui.End();
                ImGui.PopFont();
            }
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
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
