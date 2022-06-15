using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TerraAngel.Cheat
{
    public class GlobalCheatManager
    {
        public static bool AntiHurt = false;
        public static bool InfiniteMinions = false;
        public static bool InfiniteMana = false;

        public static bool Freecam = false;

        public static bool NoClip = false;
        public static float NoClipSpeed = 20.8f;
        public static int NoClipPlayerSyncTime = 3;

        public static bool FullBright = false;
        public static float FullBrightBrightness = 0.8f;

        public static bool ESPTracers = false;
        public static bool ESPBoxes = true;
        public static Color ESPTracerColor = new Color(0f, 0f, 1f);
        public static Color ESPBoxColorOthers = new Color(1f, 0f, 0f);
        public static Color ESPBoxColorLocalPlayer = new Color(0f, 1f, 0f);

        public static bool ShowTileSectionBorders = false;

        public static bool[,] LoadedTileSections;
    }
}
