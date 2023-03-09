using System;
using Terraria.GameContent;

namespace TerraAngel.WorldEdits;

public class TileSection
{
    public int Width;

    public int Height;

    public NativeTileMap? Tiles;

    public TileSection(int width, int height)
    {
        Tiles = new NativeTileMap(width, height);
        Width = width;
        Height = height;
    }

    public TileSection(int x, int y, int width, int height)
        : this(Main.tile, x, y, width, height)
    {

    }

    public TileSection(NativeTileMap copyFrom, int x, int y, int width, int height)
    {
        // do a gay little switch around so that width and height can be negative
        if (width < 0)
        {
            x += width;
            width = -width;
        }
        if (height < 0)
        {
            y += height;
            height = -height;
        }

        if (width * height <= 0)
            return;

        Tiles = new NativeTileMap(width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Tiles[i, j].CopyFrom(copyFrom[x + i, y + j]);
            }
        }

        Width = width;
        Height = height;
    }
}

public class TileSectionRenderer
{
    private static TilePaintSystemV2 paintSystem => Main.instance.TilePaintSystem;

    public static Texture2D GetTileTexture(Tile tile)
    {
        Texture2D result = TextureAssets.Tile[tile.type].Value;
        int tileType = tile.type;
        Texture2D? texture2D = paintSystem.TryGetTileAndRequestIfNotReady(tileType, 0, tile.color());
        if (texture2D is not null)
        {
            result = texture2D;
        }
        return result;
    }
    public static Texture2D GetWallDrawTexture(Tile tile)
    {
        Texture2D result = TextureAssets.Wall[tile.wall].Value;
        int wall = tile.wall;
        Texture2D? texture2D = paintSystem.TryGetWallAndRequestIfNotReady(wall, tile.wallColor());
        if (texture2D is not null)
        {
            result = texture2D;
        }
        return result;
    }

    public unsafe void DrawDetailed(TileSection section, Vector2 origin, Vector2 clipRectMin, Vector2 clipRectMax)
    {
        if (section.Width < 1 || section.Height < 1 || section.Tiles is null)
            return;

        SpriteBatch sb = Main.spriteBatch;

        Rectangle tileRectCache = new Rectangle(0, 0, 16, 16);
        Rectangle wallRectCache = new Rectangle(0, 0, 32, 32);


        origin = Vector2.Transform(origin, Main.GameViewMatrix.InverseZoomMatrix);

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
        for (int x = 0; x < section.Width; x++)
        {
            for (int y = 0; y < section.Height; y++)
            {
                if (section.Tiles[x, y].Data == null)
                    continue;

                if ((x * 16 + origin.X + 16f) < clipRectMin.X || (x + origin.X) > clipRectMax.X ||
                    (y * 16 + origin.Y + 16f) < clipRectMin.Y || (y + origin.Y) > clipRectMax.Y)
                    continue;

                Tile tile = section.Tiles[x, y];

                if (tile.wall != 0)
                {
                    Main.instance.LoadWall(tile.wall);
                    wallRectCache.X = tile.wallFrameX();
                    wallRectCache.Y = tile.wallFrameY() + Main.wallFrame[tile.wall] * 180;
                    Texture2D wallTexture = GetWallDrawTexture(tile);
                    sb.Draw(wallTexture, origin + new Vector2(x * 16, y * 16), wallRectCache, Color.White);
                }

                if (tile.active())
                {
                    Main.instance.LoadTiles(tile.type);
                    tileRectCache.X = tile.frameX + Main.tileFrame[tile.type] * 38;
                    tileRectCache.Y = tile.frameY;
                    Texture2D tileTexture = GetTileTexture(tile);
                    sb.Draw(tileTexture, origin + new Vector2(x * 16, y * 16), tileRectCache, Color.White);
                }

            }
        }
        sb.End();
    }

    public unsafe void DrawPrimitive(TileSection section, Vector2 origin, Vector2 clipRectMin, Vector2 clipRectMax)
    {
        if (section.Width < 1 || section.Height < 1 || section.Tiles is null)
            return;

        SpriteBatch sb = Main.spriteBatch;

        Rectangle rectCache = new Rectangle(0, 0, 1, 1);

        origin = Vector2.Transform(origin, Main.GameViewMatrix.InverseZoomMatrix);

        sb.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
        for (int x = 0; x < section.Width; x++)
        {
            for (int y = 0; y < section.Height; y++)
            {
                if (section.Tiles[x, y].Data == null)
                    continue;

                if ((x * 16 + origin.X + 16f) < clipRectMin.X || (x + origin.X) > clipRectMax.X ||
                    (y * 16 + origin.Y + 16f) < clipRectMin.Y || (y + origin.Y) > clipRectMax.Y)
                    continue;

                Tile tile = section.Tiles[x, y];

                if (tile.wall != 0)
                {
                    sb.Draw(GraphicsUtility.BlankTexture, new Rectangle(((int)MathF.Ceiling(origin.X + x * 16)), ((int)MathF.Ceiling(origin.Y + y * 16f)), 16, 16), rectCache, Utility.TileUtil.GetWallColor(tile.wall, tile.wallColor()));
                }

                if (tile.active())
                {
                    sb.Draw(GraphicsUtility.BlankTexture, new Rectangle(((int)MathF.Ceiling(origin.X + x * 16)), ((int)MathF.Ceiling(origin.Y + y * 16f)), 16, 16), rectCache, Utility.TileUtil.GetWallColor(tile.type, tile.color()));
                }

            }
        }
        sb.End();
    }

    public unsafe void DrawPrimitiveMap(TileSection section, Vector2 worldPoint, Vector2 clipRectMin, Vector2 clipRectMax)
    {
        if (section.Width < 1 || section.Height < 1 || section.Tiles is null)
            return;

        SpriteBatch sb = Main.spriteBatch;

        Rectangle rectCache = new Rectangle(0, 0, 1, 1);

        sb.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        for (int x = 0; x < section.Width; x++)
        {
            for (int y = 0; y < section.Height; y++)
            {
                if (section.Tiles[x, y].Data == null)
                    continue;

                Vector2 worldCoords = Util.WorldToScreenFullscreenMap(worldPoint + new Vector2(x * 16f, y * 16f));
                Vector2 worldCoords2 = Util.WorldToScreenFullscreenMap(worldPoint + new Vector2(x * 16f + 16f, y * 16f + 16f));

                if (worldCoords.X < clipRectMin.X || worldCoords2.X > clipRectMax.X ||
                    worldCoords.Y < clipRectMin.Y || worldCoords2.Y > clipRectMax.Y)
                    continue;

                Tile tile = section.Tiles[x, y];

                Rectangle rect = new Rectangle(
                    (int)MathF.Ceiling(worldCoords.X),
                    (int)MathF.Ceiling(worldCoords.Y),
                    (int)MathF.Ceiling(worldCoords2.X - worldCoords.X),
                    (int)MathF.Ceiling(worldCoords2.Y - worldCoords.Y));

                if (tile.wall != 0)
                {
                    sb.Draw(GraphicsUtility.BlankTexture, rect, rectCache, TileUtil.GetWallColor(tile.wall, tile.wallColor()));
                }

                if (tile.active())
                {
                    sb.Draw(GraphicsUtility.BlankTexture, rect, rectCache, TileUtil.GetTileColor(tile.type, tile.color()));
                }

            }
        }
        sb.End();
    }
}