using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Terraria
{
    public unsafe class Tilemap
    {
        public readonly ushort Width;
        public readonly ushort Height;

        //public readonly Tile?[,] tiles;

        public readonly TileData* tiles;

        public Tilemap(int width, int height)
        {
            Width = (ushort)width;
            Height = (ushort)height;

            //tiles = new Tile[Width, Height];
            tiles = (TileData*)Marshal.AllocHGlobal(Width * Height * sizeof(TileData));
        }

        public Tile? this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height)
                {
                    return null;
                }
                return new Tile(tiles + (x + (y * Width)));
            }
            set
            {
                //tiles[x, y] = value;
                //
                //tiles[x + (y * Width)] = value.Data;
                Buffer.MemoryCopy(value.Data, tiles + (x + (y * Width)), sizeof(TileData), sizeof(TileData));
            }
        }
    }
}
