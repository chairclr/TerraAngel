using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Xna.Framework;
using TerraAngel;
using TerraAngel.Utility;
using Terraria.Net;

namespace Terraria
{
    public unsafe class NativeTileMap : IDisposable
    {
        public readonly uint Width;
        public readonly uint Height;

        /// <summary>
        /// Size of the heap in bytes
        /// </summary>
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
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if ((uint)x >= Width || (uint)y >= Height)
                {
                    throw new IndexOutOfRangeException();
                }
                *(TileHeap + (x + (y * Width))) = *value.Data;
            }
        }
        public Tile this[Vector2i position]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                if ((uint)position.X > Width || (uint)position.Y > Height)
                {
                    throw new IndexOutOfRangeException();
                }
                return new Tile(TileHeap + (position.X + (position.Y * Width)));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if ((uint)position.X > Width || (uint)position.Y > Height)
                {
                    throw new IndexOutOfRangeException();
                }
                *(TileHeap + (position.X + (position.Y * Width))) = *value.Data;
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
        public bool InWorld(NVector2 position) => InWorld((int)(position.X / 16f), (int)(position.Y / 16f));
        public bool InWorld(Vector2 position) => InWorld((int)(position.X / 16f), (int)(position.Y / 16f));
        public bool InWorld(Point position) => InWorld(position.X, position.Y);
        public bool InWorld(Vector2i position) => InWorld(position.X, position.Y);
        public bool InWorld(int x, int y)
        {
            if ((uint)x >= Width || (uint)y >= Height)
            {
                return false;
            }
            return true;
        }

        public bool IsTileInLoadedSection(int x, int y)
        {
            if (Main.netMode == 0)
                return true;
            else if (Main.netMode == 1)
            {
                if (!CringeManager.LoadedTileSections?[x / Main.sectionWidth, y / Main.sectionHeight] ?? true)
                    return false;
            }
            return true;
        }
    }
}
