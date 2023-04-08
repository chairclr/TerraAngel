using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Xna.Framework.Input;
using Terraria.Audio;

namespace TerraAngel.Tools.Builder;

public class QuickSlopeTool : BuilderModeTool
{
    public override string Name => "Quick Slope";

    private Vector2i BeginSlopeTile = Vector2i.Zero;
    private Vector2i EndSlopeTile = Vector2i.Zero;

    public override void UpdateWhenEnabled(Tile? hoveredTile)
    {
        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

        if (InputSystem.IsKeyPressed(ClientConfig.Settings.BuilderModeQuickSlope))
        {
            BeginSlopeTile = HoveredTilePosition;
        }

        if (InputSystem.IsKeyDown(ClientConfig.Settings.BuilderModeQuickSlope))
        {
            EndSlopeTile = HoveredTilePosition;

            if (EndSlopeTile == BeginSlopeTile)
            {
                drawList.DrawTileRect(BeginSlopeTile, EndSlopeTile + new Vector2i(1, 1), Color.Red.WithAlpha(0.5f).PackedValue);
            }
            else
            {
                Util.Bresenham(BeginSlopeTile.X, BeginSlopeTile.Y, EndSlopeTile.X, EndSlopeTile.Y, (x, y) =>
                {
                    drawList.DrawTileRect(new Vector2(x, y), new Vector2(x + 1, y + 1), Color.Red.WithAlpha(0.5f).PackedValue);
                });
            }
        }

        if (InputSystem.IsKeyReleased(ClientConfig.Settings.BuilderModeQuickSlope))
        {
            EndSlopeTile = HoveredTilePosition;

            if (BeginSlopeTile == EndSlopeTile && hoveredTile.HasValue && hoveredTile.Value.active() && WorldGen.CanPoundTile(EndSlopeTile.X, EndSlopeTile.Y))
            {
                if (hoveredTile.Value.halfBrick())
                {
                    hoveredTile.Value.halfBrick(false);
                    hoveredTile.Value.slope(0);

                    SyncPoundTile(EndSlopeTile.X, EndSlopeTile.Y);
                }
                else if (hoveredTile.Value.slope() == 3)
                {
                    hoveredTile.Value.halfBrick(true);
                    hoveredTile.Value.slope(0);

                    SyncPoundTile(EndSlopeTile.X, EndSlopeTile.Y);
                }
                else
                {
                    hoveredTile.Value.slope((byte)(hoveredTile.Value.slope() + 1));

                    SyncTileSlope(EndSlopeTile.X, EndSlopeTile.Y, false);
                }

                WorldGen.SquareTileFrame(EndSlopeTile.X, EndSlopeTile.Y);

                SoundEngine.PlaySound(0, (int)HoveredPosition.X, (int)HoveredPosition.Y);
            }
            else
            {
                Util.Bresenham(BeginSlopeTile.X, BeginSlopeTile.Y, EndSlopeTile.X, EndSlopeTile.Y, (x, y) =>
                {
                    if (Main.tile.InWorld(x, y) && WorldGen.CanPoundTile(x, y))
                    {
                        Tile tile = Main.tile[x, y];

                        if (tile.halfBrick())
                        {
                            tile.halfBrick(false);
                            tile.slope(0);

                            SyncPoundTile(x, y);
                        }
                        else if (tile.slope() == 3)
                        {
                            tile.halfBrick(true);
                            tile.slope(0);

                            SyncPoundTile(x, y);
                        }
                        else
                        {
                            tile.slope((byte)(tile.slope() + 1));

                            SyncTileSlope(x, y, false);
                        }

                        WorldGen.SquareTileFrame(x, y);
                    }
                });

                SoundEngine.PlaySound(0, (int)HoveredPosition.X, (int)HoveredPosition.Y);
            }
        }
    }
}
