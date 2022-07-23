using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Terraria
{
	public class Tilemap
	{
		public readonly ushort Width;
		public readonly ushort Height;

		public readonly Tile[,] tiles;

		public Tilemap(int width, int height)
                {
			Width = (ushort)width;
			Height = (ushort)height;

			tiles = new Tile[Width, Height];
                }

		public Tile this[int x, int y]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			get
			{
				if ((uint)x >= Width || (uint)y >= Height)
				{
					return null;
				}
				return tiles[x, y];
			}
			internal set
			{
				tiles[x, y] = value;
			}
		}
	}
}
