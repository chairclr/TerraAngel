using System.Runtime.CompilerServices;
using Newtonsoft.Json.Bson;
using Terraria.DataStructures;

namespace Terraria;

public struct TileData
{
    public ushort type;
    public ushort wall;
    public ushort sTileHeader;
    public short frameX;
    public short frameY;
    public byte liquid;
    public byte bTileHeader;
    public byte bTileHeader2;
    public byte bTileHeader3;

    public void ClearEverything()
    {
        this = new TileData();
    }

    public void ClearTile()
    {
        slope(0);
        halfBrick(halfBrick: false);
        active(active: false);
        inActive(inActive: false);
    }

    public void CopyFrom(ref TileData from)
    {
        this = from;
    }

    public unsafe bool isTheSameAs(Tile compTile) => isTheSameAs(ref compTile.RefData);

    public bool isTheSameAs(ref TileData compTile)
    {
        if (sTileHeader != compTile.sTileHeader)
        {
            return false;
        }

        if (active())
        {
            if (type != compTile.type)
            {
                return false;
            }

            if (Main.tileFrameImportant[type] && (frameX != compTile.frameX || frameY != compTile.frameY))
            {
                return false;
            }
        }

        if (wall != compTile.wall || liquid != compTile.liquid)
        {
            return false;
        }

        if (compTile.liquid == 0)
        {
            if (wallColor() != compTile.wallColor())
            {
                return false;
            }

            if (wire4() != compTile.wire4())
            {
                return false;
            }
        }
        else if (bTileHeader != compTile.bTileHeader)
        {
            return false;
        }

        if (invisibleBlock() != compTile.invisibleBlock() || invisibleWall() != compTile.invisibleWall() || fullbrightBlock() != compTile.fullbrightBlock() || fullbrightWall() != compTile.fullbrightWall())
        {
            return false;
        }

        return true;
    }

    public int blockType()
    {
        if (halfBrick())
        {
            return 1;
        }

        int num = slope();
        if (num > 0)
        {
            num++;
        }

        return num;
    }

    public void liquidType(int liquidType)
    {
        switch (liquidType)
        {
            case 0:
                bTileHeader &= 159;
                break;
            case 1:
                lava(lava: true);
                break;
            case 2:
                honey(honey: true);
                break;
            case 3:
                shimmer(shimmer: true);
                break;
        }
    }

    public byte liquidType()
    {
        return (byte)((bTileHeader & 0x60) >> 5);
    }

    public bool nactive()
    {
        if ((sTileHeader & 0x60) == 32)
        {
            return true;
        }

        return false;
    }

    public void ResetToType(ushort type)
    {
        liquid = 0;
        sTileHeader = 32;
        bTileHeader = 0;
        bTileHeader2 = 0;
        bTileHeader3 = 0;
        frameX = 0;
        frameY = 0;
        this.type = type;
    }

    internal void ClearMetadata()
    {
        liquid = 0;
        sTileHeader = 0;
        bTileHeader = 0;
        bTileHeader2 = 0;
        bTileHeader3 = 0;
        frameX = 0;
        frameY = 0;
    }

    public Color actColor(Color oldColor)
    {
        if (!inActive())
        {
            return oldColor;
        }

        double num = 0.4;
        return new Color((byte)(num * (double)(int)oldColor.R), (byte)(num * (double)(int)oldColor.G), (byte)(num * (double)(int)oldColor.B), oldColor.A);
    }

    public void actColor(ref Vector3 oldColor)
    {
        if (inActive())
        {
            oldColor *= 0.4f;
        }
    }

    public bool topSlope()
    {
        byte b = slope();
        if (b != 1)
        {
            return b == 2;
        }

        return true;
    }

    public bool bottomSlope()
    {
        byte b = slope();
        if (b != 3)
        {
            return b == 4;
        }

        return true;
    }

    public bool leftSlope()
    {
        byte b = slope();
        if (b != 2)
        {
            return b == 4;
        }

        return true;
    }

    public bool rightSlope()
    {
        byte b = slope();
        if (b != 1)
        {
            return b == 3;
        }

        return true;
    }

    public bool HasSameSlope(ref TileData tile)
    {
        return (sTileHeader & 0x7400) == (tile.sTileHeader & 0x7400);
    }

    public byte wallColor()
    {
        return (byte)(bTileHeader & 0x1Fu);
    }

    public void wallColor(byte wallColor)
    {
        bTileHeader = (byte)((bTileHeader & 0xE0u) | wallColor);
    }

    public bool lava()
    {
        return (bTileHeader & 0x60) == 32;
    }

    public void lava(bool lava)
    {
        if (lava)
        {
            bTileHeader = (byte)((bTileHeader & 0x9Fu) | 0x20u);
        }
        else
        {
            bTileHeader &= 223;
        }
    }

    public bool honey()
    {
        return (bTileHeader & 0x60) == 64;
    }

    public void honey(bool honey)
    {
        if (honey)
        {
            bTileHeader = (byte)((bTileHeader & 0x9Fu) | 0x40u);
        }
        else
        {
            bTileHeader &= 191;
        }
    }

    public bool shimmer()
    {
        return (bTileHeader & 0x60) == 96;
    }

    public void shimmer(bool shimmer)
    {
        if (shimmer)
        {
            bTileHeader = (byte)((bTileHeader & 0x9Fu) | 0x60u);
        }
        else
        {
            bTileHeader &= 159;
        }
    }

    public bool wire4()
    {
        return (bTileHeader & 0x80) == 128;
    }

    public void wire4(bool wire4)
    {
        if (wire4)
        {
            bTileHeader |= 128;
        }
        else
        {
            bTileHeader &= 127;
        }
    }

    public int wallFrameX()
    {
        return (bTileHeader2 & 0xF) * 36;
    }

    public void wallFrameX(int wallFrameX)
    {
        bTileHeader2 = (byte)((bTileHeader2 & 0xF0u) | ((uint)(wallFrameX / 36) & 0xFu));
    }

    public byte frameNumber()
    {
        return (byte)((bTileHeader2 & 0x30) >> 4);
    }

    public void frameNumber(byte frameNumber)
    {
        bTileHeader2 = (byte)((bTileHeader2 & 0xCFu) | (uint)((frameNumber & 3) << 4));
    }

    public byte wallFrameNumber()
    {
        return (byte)((bTileHeader2 & 0xC0) >> 6);
    }

    public void wallFrameNumber(byte wallFrameNumber)
    {
        bTileHeader2 = (byte)((bTileHeader2 & 0x3Fu) | (uint)((wallFrameNumber & 3) << 6));
    }

    public int wallFrameY()
    {
        return (bTileHeader3 & 7) * 36;
    }

    public void wallFrameY(int wallFrameY)
    {
        bTileHeader3 = (byte)((bTileHeader3 & 0xF8u) | ((uint)(wallFrameY / 36) & 7u));
    }

    public bool checkingLiquid()
    {
        return (bTileHeader3 & 8) == 8;
    }

    public void checkingLiquid(bool checkingLiquid)
    {
        if (checkingLiquid)
        {
            bTileHeader3 |= 8;
        }
        else
        {
            bTileHeader3 &= 247;
        }
    }

    public bool skipLiquid()
    {
        return (bTileHeader3 & 0x10) == 16;
    }

    public void skipLiquid(bool skipLiquid)
    {
        if (skipLiquid)
        {
            bTileHeader3 |= 16;
        }
        else
        {
            bTileHeader3 &= 239;
        }
    }

    public bool invisibleBlock()
    {
        return (bTileHeader3 & 0x20) == 32;
    }

    public void invisibleBlock(bool invisibleBlock)
    {
        if (invisibleBlock)
        {
            bTileHeader3 |= 32;
        }
        else
        {
            bTileHeader3 = (byte)(bTileHeader3 & 0xFFFFFFDFu);
        }
    }

    public bool invisibleWall()
    {
        return (bTileHeader3 & 0x40) == 64;
    }

    public void invisibleWall(bool invisibleWall)
    {
        if (invisibleWall)
        {
            bTileHeader3 |= 64;
        }
        else
        {
            bTileHeader3 = (byte)(bTileHeader3 & 0xFFFFFFBFu);
        }
    }

    public bool fullbrightBlock()
    {
        return (bTileHeader3 & 0x80) == 128;
    }

    public void fullbrightBlock(bool fullbrightBlock)
    {
        if (fullbrightBlock)
        {
            bTileHeader3 |= 128;
        }
        else
        {
            bTileHeader3 = (byte)(bTileHeader3 & 0xFFFFFF7Fu);
        }
    }

    public byte color()
    {
        return (byte)(sTileHeader & 0x1Fu);
    }

    public void color(byte color)
    {
        sTileHeader = (ushort)((sTileHeader & 0xFFE0u) | color);
    }

    public bool active()
    {
        return (sTileHeader & 0x20) == 32;
    }

    public void active(bool active)
    {
        if (active)
        {
            sTileHeader |= 32;
        }
        else
        {
            sTileHeader &= 65503;
        }
    }

    public bool inActive()
    {
        return (sTileHeader & 0x40) == 64;
    }

    public void inActive(bool inActive)
    {
        if (inActive)
        {
            sTileHeader |= 64;
        }
        else
        {
            sTileHeader &= 65471;
        }
    }

    public bool wire()
    {
        return (sTileHeader & 0x80) == 128;
    }

    public void wire(bool wire)
    {
        if (wire)
        {
            sTileHeader |= 128;
        }
        else
        {
            sTileHeader &= 65407;
        }
    }

    public bool wire2()
    {
        return (sTileHeader & 0x100) == 256;
    }

    public void wire2(bool wire2)
    {
        if (wire2)
        {
            sTileHeader |= 256;
        }
        else
        {
            sTileHeader &= 65279;
        }
    }

    public bool wire3()
    {
        return (sTileHeader & 0x200) == 512;
    }

    public void wire3(bool wire3)
    {
        if (wire3)
        {
            sTileHeader |= 512;
        }
        else
        {
            sTileHeader &= 65023;
        }
    }

    public bool halfBrick()
    {
        return (sTileHeader & 0x400) == 1024;
    }

    public void halfBrick(bool halfBrick)
    {
        if (halfBrick)
        {
            sTileHeader |= 1024;
        }
        else
        {
            sTileHeader &= 64511;
        }
    }

    public bool actuator()
    {
        return (sTileHeader & 0x800) == 2048;
    }

    public void actuator(bool actuator)
    {
        if (actuator)
        {
            sTileHeader |= 2048;
        }
        else
        {
            sTileHeader &= 63487;
        }
    }

    public byte slope()
    {
        return (byte)((sTileHeader & 0x7000) >> 12);
    }

    public void slope(byte slope)
    {
        sTileHeader = (ushort)((sTileHeader & 0x8FFFu) | (uint)((slope & 7) << 12));
    }

    public bool fullbrightWall()
    {
        return (sTileHeader & 0x8000) == 32768;
    }

    public void fullbrightWall(bool fullbrightWall)
    {
        if (fullbrightWall)
        {
            sTileHeader |= 32768;
        }
        else
        {
            sTileHeader = (ushort)(sTileHeader & 0xFFFF7FFFu);
        }
    }

    public void Clear(TileDataType types)
    {
        if ((types & TileDataType.Tile) != 0)
        {
            type = 0;
            active(active: false);
            frameX = 0;
            frameY = 0;
        }

        if ((types & TileDataType.Wall) != 0)
        {
            wall = 0;
            wallFrameX(0);
            wallFrameY(0);
        }

        if ((types & TileDataType.TilePaint) != 0)
        {
            ClearBlockPaintAndCoating();
        }

        if ((types & TileDataType.WallPaint) != 0)
        {
            ClearWallPaintAndCoating();
        }

        if ((types & TileDataType.Liquid) != 0)
        {
            liquid = 0;
            liquidType(0);
            checkingLiquid(checkingLiquid: false);
        }

        if ((types & TileDataType.Slope) != 0)
        {
            slope(0);
            halfBrick(halfBrick: false);
        }

        if ((types & TileDataType.Wiring) != 0)
        {
            wire(wire: false);
            wire2(wire2: false);
            wire3(wire3: false);
            wire4(wire4: false);
        }

        if ((types & TileDataType.Actuator) != 0)
        {
            actuator(actuator: false);
            inActive(inActive: false);
        }
    }

    public static void SmoothSlope(int x, int y, bool applyToNeighbors = true, bool sync = false)
    {
        if (applyToNeighbors)
        {
            SmoothSlope(x + 1, y, applyToNeighbors: false, sync);
            SmoothSlope(x - 1, y, applyToNeighbors: false, sync);
            SmoothSlope(x, y + 1, applyToNeighbors: false, sync);
            SmoothSlope(x, y - 1, applyToNeighbors: false, sync);
        }

        Tile tile = Main.tile[x, y];
        if (!WorldGen.CanPoundTile(x, y) || !WorldGen.SolidOrSlopedTile(x, y))
        {
            return;
        }

        bool flag = !WorldGen.TileEmpty(x, y - 1);
        bool flag2 = !WorldGen.SolidOrSlopedTile(x, y - 1) && flag;
        bool flag3 = WorldGen.SolidOrSlopedTile(x, y + 1);
        bool flag4 = WorldGen.SolidOrSlopedTile(x - 1, y);
        bool flag5 = WorldGen.SolidOrSlopedTile(x + 1, y);
        int num = ((flag ? 1 : 0) << 3) | ((flag3 ? 1 : 0) << 2) | ((flag4 ? 1 : 0) << 1) | (flag5 ? 1 : 0);
        bool flag6 = tile.halfBrick();
        int num2 = tile.slope();
        switch (num)
        {
            case 10:
                if (!flag2)
                {
                    tile.halfBrick(halfBrick: false);
                    tile.slope(3);
                }

                break;
            case 9:
                if (!flag2)
                {
                    tile.halfBrick(halfBrick: false);
                    tile.slope(4);
                }

                break;
            case 6:
                tile.halfBrick(halfBrick: false);
                tile.slope(1);
                break;
            case 5:
                tile.halfBrick(halfBrick: false);
                tile.slope(2);
                break;
            case 4:
                tile.slope(0);
                tile.halfBrick(halfBrick: true);
                break;
            default:
                tile.halfBrick(halfBrick: false);
                tile.slope(0);
                break;
        }

        if (sync)
        {
            int num3 = tile.slope();
            bool flag7 = flag6 != tile.halfBrick();
            bool flag8 = num2 != num3;
            if (flag7 && flag8)
            {
                NetMessage.SendData(17, -1, -1, null, 23, x, y, num3);
            }
            else if (flag7)
            {
                NetMessage.SendData(17, -1, -1, null, 7, x, y, 1f);
            }
            else if (flag8)
            {
                NetMessage.SendData(17, -1, -1, null, 14, x, y, num3);
            }
        }
    }

    public void CopyPaintAndCoating(Tile other)
    {
        color(other.color());
        invisibleBlock(other.invisibleBlock());
        fullbrightBlock(other.fullbrightBlock());
    }

    public void CopyPaintAndCoating(ref TileData other)
    {
        color(other.color());
        invisibleBlock(other.invisibleBlock());
        fullbrightBlock(other.fullbrightBlock());
    }

    public TileColorCache BlockColorAndCoating()
    {
        TileColorCache result = default(TileColorCache);
        result.Color = color();
        result.FullBright = fullbrightBlock();
        result.Invisible = invisibleBlock();
        return result;
    }

    public TileColorCache WallColorAndCoating()
    {
        TileColorCache result = default(TileColorCache);
        result.Color = wallColor();
        result.FullBright = fullbrightWall();
        result.Invisible = invisibleWall();
        return result;
    }

    public void ClearBlockPaintAndCoating()
    {
        color(0);
        fullbrightBlock(fullbrightBlock: false);
        invisibleBlock(invisibleBlock: false);
    }

    public void ClearWallPaintAndCoating()
    {
        wallColor(0);
        fullbrightWall(fullbrightWall: false);
        invisibleWall(invisibleWall: false);
    }

    public override string ToString()
    {
        return "Tile Type:" + type + " Active:" + active().ToString() + " Wall:" + wall + " Slope:" + slope() + " fX:" + frameX + " fY:" + frameY;
    }
}
