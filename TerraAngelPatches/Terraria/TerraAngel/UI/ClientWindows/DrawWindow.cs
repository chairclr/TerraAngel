using System;
using TerraAngel.Tools.Visuals;
using TerraAngel.WorldEdits;

namespace TerraAngel.UI.ClientWindows;

public class DrawWindow : ClientWindow
{
    public override bool IsToggleable => false;

    public override bool DefaultEnabled => true;

    public override bool IsEnabled => true;

    public override bool IsGlobalToggle => false;

    public override void Draw(ImGuiIOPtr io)
    {
        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
        if (!Main.gameMenu)
        {
            WorldEdit? worldEdit = ClientLoader.MainRenderer?.CurrentWorldEdit;
            ESPTool esp = ToolManager.GetTool<ESPTool>();


            if (!Main.mapFullscreen)
            {
                worldEdit?.DrawPreviewInWorld(io, drawList);

                if (ToolManager.GetTool<HeldItemViewerTool>().Enabled)
                {
                    for (int i = 0; i < 255; i++)
                    {
                        Player player = Main.player[i];
                        if (player.active && player.whoAmI != Main.myPlayer)
                        {
                            if (player.selectedItem < 59)
                            {
                                Item item = player.inventory[player.selectedItem];

                                Vector2 drawCenter = Util.WorldToScreenWorld(player.Top - new Vector2(0f, 24f));

                                if (item.type != 0)
                                {
                                    ImGuiUtil.DrawItemCentered(drawList, item, drawCenter, 24f);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                worldEdit?.DrawPreviewInMap(io, drawList);
            }

            if (esp.DrawAnyESP)
            {
                if (esp.MapESP || !Main.mapFullscreen)
                {
                    Vector2 localPlayerCenter = Util.WorldToScreenDynamic(Main.LocalPlayer.Center);
                    for (int i = 0; i < 1000; i++)
                    {
                        if (i < 255)
                        {
                            if (Main.player[i].active)
                            {
                                Player currentPlayer = Main.player[i];
                                if (esp.PlayerBoxes)
                                {
                                    Vector2 minScreenPos = Util.WorldToScreenDynamicPixelPerfect(currentPlayer.TopLeft);
                                    Vector2 maxScreenPos = Util.WorldToScreenDynamicPixelPerfect(currentPlayer.BottomRight);

                                    Color drawColor = esp.OtherPlayerColor;

                                    if (currentPlayer.whoAmI == Main.myPlayer)
                                        drawColor = esp.LocalPlayerColor;
                                    if (currentPlayer.TerraAngelUser)
                                        drawColor = esp.OtherTerraAngelUserColor;

                                    drawList.AddRect(minScreenPos, maxScreenPos, drawColor.PackedValue);
                                }
                                if (esp.PlayerTracers)
                                {
                                    if (currentPlayer.whoAmI != Main.myPlayer)
                                    {
                                        Vector2 otherPlayerCenter = Util.WorldToScreenDynamic(currentPlayer.Center);

                                        drawList.AddLine(localPlayerCenter, otherPlayerCenter, esp.TracerColor.PackedValue);
                                    }
                                }
                            }
                        }
                        if (i < 201)
                        {
                            if (Main.npc[i].active)
                            {
                                NPC currentNPC = Main.npc[i];
                                if (esp.NPCBoxes)
                                {
                                    // as per request of an anonymous user, NPC net offset drawing
                                    if (!currentNPC.position.HasNaNs())
                                    {
                                        Vector2 minNetScreenPos = Util.WorldToScreenDynamicPixelPerfect(currentNPC.TopLeft);
                                        Vector2 maxNetScreenPos = Util.WorldToScreenDynamicPixelPerfect(currentNPC.BottomRight);
                                        drawList.AddRect(minNetScreenPos, maxNetScreenPos, esp.NPCNetOffsetColor.PackedValue);

                                        Vector2 minScreenPos = Util.WorldToScreenDynamicPixelPerfect(currentNPC.TopLeft + currentNPC.netOffset);
                                        Vector2 maxScreenPos = Util.WorldToScreenDynamicPixelPerfect(currentNPC.BottomRight + currentNPC.netOffset);
                                        drawList.AddRect(minScreenPos, maxScreenPos, esp.NPCColor.PackedValue);
                                    }
                                }
                            }
                        }
                        if (i < 400)
                        {
                            if (Main.item[i].active && Main.item[i].stack > 0)
                            {
                                Item currentItem = Main.item[i];
                                if (esp.ItemBoxes)
                                {
                                    Vector2 minScreenPos = Util.WorldToScreenDynamicPixelPerfect(currentItem.TopLeft);
                                    Vector2 maxScreenPos = Util.WorldToScreenDynamicPixelPerfect(currentItem.BottomRight);
                                    // dont draw if its off screen lol
                                    if (Util.IsRectOnScreen(minScreenPos, maxScreenPos, io.DisplaySize))
                                    {
                                        drawList.AddRect(minScreenPos, maxScreenPos, esp.ItemColor.PackedValue);
                                    }
                                }
                            }
                        }
                        if (Main.projectile[i].active)
                        {
                            Projectile currentProjectile = Main.projectile[i];
                            if (esp.ProjectileBoxes)
                            {

                                Rectangle myRect = new Rectangle((int)currentProjectile.position.X, (int)currentProjectile.position.Y, currentProjectile.width, currentProjectile.height);

                                if (currentProjectile.type == 101)
                                {
                                    myRect.Inflate(30, 30);
                                }

                                if (currentProjectile.type == 85)
                                {
                                    if (currentProjectile.localAI[0] >= 54f)
                                    {
                                        return;
                                    }

                                    int num = (int)Utils.Remap(currentProjectile.localAI[0], 0f, 72f, 10f, 40f);
                                    myRect.Inflate(num, num);
                                }

                                if (currentProjectile.type == 188)
                                {
                                    myRect.Inflate(20, 20);
                                }

                                if (currentProjectile.aiStyle == 29)
                                {
                                    myRect.Inflate(4, 4);
                                }

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

                                Vector2 minScreenPos = Util.WorldToScreenDynamicPixelPerfect(myRect.TopLeft());
                                Vector2 maxScreenPos = Util.WorldToScreenDynamicPixelPerfect(myRect.BottomRight());

                                if (Util.IsRectOnScreen(minScreenPos, maxScreenPos, io.DisplaySize))
                                {
                                    drawList.AddRect(minScreenPos, maxScreenPos, esp.ProjectileColor.PackedValue);
                                }
                            }
                        }
                    }
                }

                if (esp.ShowTileSections)
                {
                    for (int xs = 0; xs < Main.maxSectionsX; xs++)
                    {
                        for (int ys = 0; ys < Main.maxSectionsY; ys++)
                        {
                            Color col = new Color(1f, 1f, 0f);
                            if (!Main.tile.IsTileSectionLoaded(xs, ys))
                            {
                                col = new Color(1f, 0f, 0f);
                            }

                            Vector2 worldCoords = new Vector2(xs * 200f * 16f, ys * 150f * 16f);
                            Vector2 worldCoords2 = new Vector2((xs + 1f) * 200f * 16f, (ys + 1f) * 150f * 16f);

                            drawList.AddRect(Util.WorldToScreenDynamic(worldCoords), Util.WorldToScreenDynamic(worldCoords2), col.PackedValue);
                        }
                    }
                }
            }

            if (worldEdit is not null)
            {
                Vector2 mousePos = Util.ScreenToWorldDynamic(InputSystem.MousePosition) / 16f;

                if (worldEdit.RunEveryFrame)
                {
                    if (InputSystem.MiddleMouseDown)
                    {
                        worldEdit.Edit(mousePos);
                    }
                }
                else if (InputSystem.MiddleMousePressed)
                {
                    worldEdit.Edit(mousePos);
                }
            }

            for (int i = 0; i < (ClientLoader.MainRenderer?.WorldEdits.Count ?? 0); i++)
            {
                ClientLoader.MainRenderer?.WorldEdits[i].Update();
            }

            ToolManager.Update();
        }
    }
}
