using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraria
{
    public struct TileData
    {
        public ushort type;
        public ushort wall;
        public short frameX;
        public short frameY;
        public short sTileHeader;
        public byte liquid;
        public byte bTileHeader;
        public byte bTileHeader2;
        public byte bTileHeader3;
    }
}
