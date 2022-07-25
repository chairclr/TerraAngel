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

namespace TerraAngel.Client.ClientWindows
{
    public class DrawWindow : ClientWindow
    {
        public override bool IsToggleable => false;
        public override bool DefaultEnabled => true;
        public override bool IsEnabled { get => true; }



        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.Begin("DRAWWINDOW", ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);
            ImGui.PushClipRect(NVector2.Zero, io.DisplaySize, false);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (!Main.gameMenu)
            {
                if (!Main.mapFullscreen)
                {
                    ESPBoxesCringe espBoxes = CringeManager.GetCringe<ESPBoxesCringe>();
                    ESPTracersCringe espTracers = CringeManager.GetCringe<ESPTracersCringe>();
                    Vector2 localPlayerCenter = Util.WorldToScreen(Main.LocalPlayer.Center);
                    for (int i = 0; i < 1000; i++)
                    {
                        if (i < 255)
                        {
                            if (Main.player[i].active)
                            {
                                Player currentPlayer = Main.player[i];
                                if (espBoxes.PlayerBoxes)
                                {
                                    Vector2 minScreenPos = Util.WorldToScreenExact(currentPlayer.TopLeft);
                                    Vector2 maxScreenPos = Util.WorldToScreenExact(currentPlayer.BottomRight);
                                    if (currentPlayer.whoAmI == Main.myPlayer)
                                    {
                                        drawList.AddRect(minScreenPos.ToNumerics(), maxScreenPos.ToNumerics(), espBoxes.LocalPlayerColor.PackedValue);
                                    }
                                    else
                                    {
                                        drawList.AddRect(minScreenPos.ToNumerics(), maxScreenPos.ToNumerics(), espBoxes.OtherPlayerColor.PackedValue);
                                    }
                                }
                                if (espTracers.Enabled)
                                {
                                    if (currentPlayer.whoAmI != Main.myPlayer)
                                    {
                                        Vector2 otherPlayerCenter = Util.WorldToScreen(currentPlayer.Center);

                                        drawList.AddLine(localPlayerCenter.ToNumerics(), otherPlayerCenter.ToNumerics(), espTracers.TracerColor.PackedValue);
                                    }
                                }
                            }
                        }
                        if (i < 201)
                        {
                            if (Main.npc[i].active)
                            {
                                NPC currentNPC = Main.npc[i];
                                if (espBoxes.NPCBoxes)
                                {
                                    // as per request of an anonymous user, NPC net offset drawing
                                    Vector2 minNetScreenPos = Util.WorldToScreenExact(currentNPC.TopLeft);
                                    Vector2 maxNetScreenPos = Util.WorldToScreenExact(currentNPC.BottomRight);
                                    drawList.AddRect(minNetScreenPos.ToNumerics(), maxNetScreenPos.ToNumerics(), espBoxes.NPCNetOffsetColor.PackedValue);


                                    Vector2 minScreenPos = Util.WorldToScreenExact(currentNPC.TopLeft + currentNPC.netOffset);
                                    Vector2 maxScreenPos = Util.WorldToScreenExact(currentNPC.BottomRight + currentNPC.netOffset);
                                    drawList.AddRect(minScreenPos.ToNumerics(), maxScreenPos.ToNumerics(), espBoxes.NPCColor.PackedValue);

                                }
                            }
                        }
                        if (Main.projectile[i].active)
                        {
                            Projectile currentProjectile = Main.projectile[i];
                            if (espBoxes.ProjectileBoxes)
                            {

                                Rectangle myRect = new Rectangle((int)currentProjectile.position.X, (int)currentProjectile.position.Y, currentProjectile.width, currentProjectile.height);
                                if (currentProjectile.type == 85 || currentProjectile.type == 101)
                                {
                                    int num = 30;
                                    myRect.X -= num;
                                    myRect.Y -= num;
                                    myRect.Width += num * 2;
                                    myRect.Height += num * 2;
                                }

                                if (currentProjectile.type == 188)
                                {
                                    int num2 = 20;
                                    myRect.X -= num2;
                                    myRect.Y -= num2;
                                    myRect.Width += num2 * 2;
                                    myRect.Height += num2 * 2;
                                }

                                if (currentProjectile.aiStyle == 29)
                                {
                                    int num3 = 4;
                                    myRect.X -= num3;
                                    myRect.Y -= num3;
                                    myRect.Width += num3 * 2;
                                    myRect.Height += num3 * 2;
                                }

                                Vector2 minScreenPos = Util.WorldToScreenExact(myRect.TopLeft());
                                Vector2 maxScreenPos = Util.WorldToScreenExact(myRect.BottomRight());

                                

                                // dont draw if its off screen
                                if (minScreenPos.X > 0 || minScreenPos.X < io.DisplaySize.X ||
                                    minScreenPos.Y > 0 || minScreenPos.Y < io.DisplaySize.Y ||
                                    maxScreenPos.X > 0 || maxScreenPos.X < io.DisplaySize.X ||
                                    maxScreenPos.Y > 0 || maxScreenPos.Y < io.DisplaySize.Y)
                                {
                                    drawList.AddRect(minScreenPos.ToNumerics(), maxScreenPos.ToNumerics(), espBoxes.ProjectileColor.PackedValue);
                                }
                            }
                        }
                    }

                    WorldEdit worldEdit = ClientLoader.MainRenderer.CurrentWorldEdit;
                    worldEdit?.DrawPreviewInWorld(io, drawList);

                    if (CringeManager.GetCringe<HeldItemViewerCringe>().Enabled)
                    {
                        for (int i = 0; i < 255; i++)
                        {
                            Player player = Main.player[i];
                            if (player.active && player.whoAmI != Main.myPlayer)
                            {
                                Item item = new Item();

                                if (player.selectedItem < 59)
                                    item = player.inventory[player.selectedItem];

                                Vector2 drawCenter = Util.WorldToScreen(player.Top - new Vector2(0f, 24f));

                                if (item.type != 0)
                                    ImGuiUtil.DrawItemCentered(drawList, item.type, drawCenter, 24f, item.stack == 1 ? 0 : item.stack);
                            }
                        }
                    }
                }
                else
                {
                    WorldEdit worldEdit = ClientLoader.MainRenderer.CurrentWorldEdit;
                    worldEdit?.DrawPreviewInMap(io, drawList);


                }

                {
                    WorldEdit worldEdit = ClientLoader.MainRenderer.CurrentWorldEdit;
                    if (worldEdit != null)
                    {
                        Vector2 mousePos = Util.ScreenToWorld(Input.InputSystem.MousePosition) / 16f;

                        if (Main.mapFullscreen)
                            mousePos = Util.ScreenToWorldFullscreenMap(Input.InputSystem.MousePosition) / 16f;

                        mousePos = new Vector2(MathF.Floor(mousePos.X), MathF.Floor(mousePos.Y));

                        if (worldEdit.RunEveryFrame)
                        {
                            if (Input.InputSystem.MiddleMouseDown)
                            {
                                worldEdit.Edit(mousePos);
                            }
                        }
                        else if (Input.InputSystem.MiddleMousePressed)
                        {
                            worldEdit.Edit(mousePos);
                        }
                    }
                }

                if (CringeManager.GetCringe<ShowTileSectionsCringe>().Enabled)
                {
                    if (CringeManager.LoadedTileSections != null)
                    {
                        for (int xs = 0; xs < Main.maxSectionsX; xs++)
                        {
                            for (int ys = 0; ys < Main.maxSectionsY; ys++)
                            {
                                Color col = new Color(1f, 1, 0f);
                                if (!CringeManager.LoadedTileSections[xs, ys])
                                {
                                    col = new Color(1f, 0f, 0f);
                                }

                                Vector2 worldCoords = new Vector2(xs * 200 * 16, ys * 150 * 16);
                                Vector2 worldCoords2 = new Vector2((xs + 1) * 200 * 16, (ys + 1) * 150 * 16);

                                if (Main.mapFullscreen)
                                {
                                    drawList.AddRect(Util.WorldToScreenFullscreenMap(worldCoords).ToNumerics(), Util.WorldToScreenFullscreenMap(worldCoords2).ToNumerics(), col.PackedValue);
                                }
                                else
                                {
                                    drawList.AddRect(Util.WorldToScreen(worldCoords).ToNumerics(), Util.WorldToScreen(worldCoords2).ToNumerics(), col.PackedValue);
                                }
                            }
                        }
                    }
                }
            }

            ImGui.PopClipRect();
            ImGui.End();
        }

    }
}
