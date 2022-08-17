using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        public Tile? this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height)
                {
                    return null;
                }
                return new Tile(TileHeap + (x + (y * Width)));
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
