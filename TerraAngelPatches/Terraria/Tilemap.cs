using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using TerraAngel;

namespace Terraria
{
    public unsafe class NativeTileMap : IDisposable
    {
        public readonly uint Width;
        public readonly uint Height;
        public readonly long HeapSize;
        public readonly TileData* TileHeap;

        public NativeTileMap(int width, int height)
        {
            Width = (uint)width;
            Height = (uint)height;
            HeapSize = Width * Height * sizeof(TileData);

            TileHeap = (TileData*)Marshal.AllocHGlobal((IntPtr)HeapSize);

            // tell the GC that we just allocated a bunch of unmanaged memory
            GC.AddMemoryPressure(HeapSize);
        }

        public Tile this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                if ((uint)x >= Width || (uint)y >= Height)
                { 
                    throw new IndexOutOfRangeException();
                }
                return new Tile(TileHeap + (x + (y * Width)));
            }
            set
            {
                // im not sure why this even is here? can we get rid of this?
                *(TileHeap + (x + (y * Width))) = *value.Data;
            }
        }

        public void ResetHeap()
        {
            Unsafe.InitBlockUnaligned(TileHeap, 0, (uint)HeapSize);
        }

        public void Dispose()
        {
            GC.RemoveMemoryPressure(HeapSize);
            Marshal.FreeHGlobal((IntPtr)TileHeap);
        }
    }
}
