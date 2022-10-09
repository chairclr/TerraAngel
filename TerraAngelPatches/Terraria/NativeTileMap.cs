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
    public unsafe class NativeTileMap
    {
        public readonly uint Width;
        public readonly uint Height;

        /// <summary>
        /// Size of the heap in bytes
        /// </summary>
        public readonly long HeapSize;

        public readonly TileData[,] TileHeap;
        public readonly TileData* TileHeapPtr;

        public bool[,] LoadedTileSections = new bool[0, 0];

        public NativeTileMap(int width, int height)
        {
            Width = (uint)width;
            Height = (uint)height;
            HeapSize = Width * Height * sizeof(TileData);

            TileHeap = new TileData[Width, Height];
            TileHeapPtr = (TileData*)Unsafe.AsPointer(ref TileHeap[0, 0]);

            LoadedTileSections = new bool[Width / Main.sectionWidth, Height / Main.sectionHeight];
        }

        public Tile this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                return new Tile((TileData*)Unsafe.AsPointer(ref TileHeap[x, y]));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                TileHeap[x, y] = *value.Data;
            }
        }
        public Tile this[Vector2i position]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                return new Tile((TileData*)Unsafe.AsPointer(ref TileHeap[position.X, position.Y]));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                TileHeap[position.X, position.Y] = *value.Data;
            }
        }

        public void ResetHeap()
        {
            Unsafe.InitBlockUnaligned(TileHeapPtr, 0, (uint)HeapSize);
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

        public bool IsTileSectionLoaded(int sectionX, int sectionY)
        {
            if (Main.netMode == 0) return true;
            if (Main.netMode == 1) return LoadedTileSections[sectionX, sectionY];
            return true;
        }
        public bool IsTileInLoadedSection(int x, int y)
        {
            if (Main.netMode == 0)
                return true;
            else if (Main.netMode == 1)
            {
                return IsTileSectionLoaded(x / Main.sectionWidth, y / Main.sectionHeight);
            }
            return true;
        }
    }
}
