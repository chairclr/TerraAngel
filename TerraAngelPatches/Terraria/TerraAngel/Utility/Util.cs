using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerraAngel.Utility
{
    public class Util
    {
        private static string[] ByteSizeNames = { "b", "k", "m", "g", "t", "p" };

        public static string PrettyPrintBytes(long bytes, string format = "{0:F2}{1}")
        {
            float len = bytes;
            int order = 0;
            while ((len >= 1024 || len >= 100f) && order < ByteSizeNames.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return string.Format(format, len, ByteSizeNames[order]);
        }


        public static Vector2 ScreenToWorldFullscreenMap(Vector2 screenPoint)
        {
            Vector2 screenPointScaled = screenPoint;
            screenPointScaled.X -= Main.screenWidth / 2;
            screenPointScaled.Y -= Main.screenHeight / 2;
            Vector2 mapFullscreenPos = Main.mapFullscreenPos;
            Vector2 finalPosition = mapFullscreenPos;
            screenPointScaled /= 16f;
            screenPointScaled *= 16f / Main.mapFullscreenScale;
            finalPosition += screenPointScaled;
            finalPosition *= 16f;
            if (finalPosition.X < 0f)
            {
                finalPosition.X = 0f;
            }
            if (finalPosition.Y < 0f)
            {
                finalPosition.Y = 0f;
            }

            return finalPosition;
        }

        public static Vector2 WorldToScreenFullscreenMap(Vector2 worldPoint)
        {
            float xScaled = (worldPoint.X * Main.mapFullscreenScale) / 16;
            float yScaled = (worldPoint.Y * Main.mapFullscreenScale) / 16;
            xScaled -= Main.mapFullscreenPos.X * Main.mapFullscreenScale;
            yScaled -= Main.mapFullscreenPos.Y * Main.mapFullscreenScale;
            xScaled += Main.screenWidth / 2;
            yScaled += Main.screenHeight / 2;
            return new Vector2(xScaled, yScaled);
        }

        public static Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);
        }
        public static Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition + Main.screenPosition, Matrix.Invert(Main.GameViewMatrix.ZoomMatrix));
        }
    }
}
