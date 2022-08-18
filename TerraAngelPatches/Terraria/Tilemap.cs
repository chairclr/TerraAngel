using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using TerraAngel;

namespace Terraria
{
    public unsafe class Tilemap : IDisposable
    {
        public readonly ushort Width;
        public readonly ushort Height;
        public readonly TileData* TileHeap;

        public Tilemap(int width, int height)
        {
            Width = (ushort)width;
            Height = (ushort)height;

            TileHeap = (TileData*)Marshal.AllocHGlobal(Width * Height * sizeof(TileData));
        }

        [ThreadStatic]
        private static Tile swapTile = new Tile();

        public Tile this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height)
                {
                    return new Tile();
                }
                swapTile.Data = TileHeap + (x + (y * Width));
                return swapTile;
            }
            set
            {
                // im not sure why this even is here? can we get rid of this?
                *(TileHeap + (x + (y * Width))) = *value.Data;
            }
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)TileHeap);
        }
    }
}
