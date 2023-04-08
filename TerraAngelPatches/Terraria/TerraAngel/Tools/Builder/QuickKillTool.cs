using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using TerraAngel.ID;

namespace TerraAngel.Tools.Builder;

public class QuickKillTool : BuilderModeTool
{
    private bool Selecting = false;

    private Vector2i BeginKillTile = Vector2i.Zero;
    private Vector2i EndKillTile = Vector2i.Zero;

    public override void UpdateWhenEnabled(Tile? hoveredTile)
    {
        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

        if (InputSystem.RightMousePressed && !InputSystem.Ctrl && !ImGui.GetIO().WantCaptureMouse)
        {
            BeginKillTile = HoveredTilePosition;
            Selecting = true;
        }

        if (Selecting)
        {
            if (InputSystem.RightMouseDown)
            {
                EndKillTile = HoveredTilePosition;

                if (InputSystem.Alt)
                {
                    //drawList.DrawTileRect(new Vector2(BeginKillTile.X, BeginKillTile.Y), new Vector2(EndKillTile.X + 1, EndKillTile.Y + 1), Color.Red.WithAlpha(0.5f).PackedValue);
                }
                else
                {
                    Util.Bresenham(BeginKillTile.X, BeginKillTile.Y, EndKillTile.X, EndKillTile.Y, (x, y) =>
                    {
                        drawList.DrawTileRect(new Vector2(x, y), new Vector2(x + 1, y + 1), Color.Red.WithAlpha(0.5f).PackedValue);
                    });
                }
            }

            if (InputSystem.RightMouseReleased)
            {
                EndKillTile = HoveredTilePosition;

                if (InputSystem.Alt)
                {
                    
                }
                else
                {
                    Util.Bresenham(BeginKillTile.X, BeginKillTile.Y, EndKillTile.X, EndKillTile.Y, (x, y) =>
                    {
                        KillTile(ref Main.tile.GetTileRef(x, y), x, y);
                    });
                }
            }
        }
    }

    public void KillTile(ref TileData tile, int x, int y)
    {
        if (WorldGen.CanKillTile(x, y))
        {
            tile.active(false);
            tile.type = 0;
            NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.KillTileNoItem, number2: x, number3: y);
            WorldGen.SquareTileFrame(x, y);
        }
    }
}
