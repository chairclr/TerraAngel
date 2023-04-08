using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using TerraAngel.ID;
using Terraria.Audio;

namespace TerraAngel.Tools.Builder;

public abstract class BuilderModeTool : Tool
{
    public static bool Enabled = false;

    public override ToolTabs Tab => ToolTabs.None;

    protected Vector2 HoveredPosition => Util.ScreenToWorldWorld(InputSystem.MousePosition);

    protected Vector2i HoveredTilePosition => (Util.ScreenToWorldWorld(InputSystem.MousePosition) / 16f);

    public override void Update()
    { 
        if (Enabled && !Main.gameMenu)
        {
            Tile? hoveredTile = null;

            if (Main.tile.InWorld(HoveredTilePosition))
            {
                hoveredTile = Main.tile[HoveredTilePosition];
            }

            UpdateWhenEnabled(hoveredTile);
        }
    }

    public abstract void UpdateWhenEnabled(Tile? hoveredTile);

    protected void SyncTileSlope(int x, int y, bool pound)
    {
        if (pound)
        {
            NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.PoundTile, number2: x, number3: y);
        }

        NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.SlopeTile, number2: x, number3: y, number4: Main.tile[x, y].slope());
    }

    protected void SyncPoundTile(int x, int y)
    {
        NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.SlopePoundTile, number2: x, number3: y, number4: Main.tile[x, y].slope());
    }
}
