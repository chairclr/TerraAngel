using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public static string ToSentenceCase(string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }

        public static string[] EnumFancyNames(Type type)
        {
            return Enum.GetNames(type).Select((x) => Util.ToSentenceCase(x)).ToArray();
        }
        public static string[] EnumFancyNames<TEnum>() 
        {
            return Enum.GetNames(typeof(TEnum)).Select((x) => Util.ToSentenceCase(x)).ToArray();
        } 
    }
    public static class VectorHelper
    {
        public static System.Numerics.Vector2 ToNumerics(this Vector2 v)
        {
            return new System.Numerics.Vector2(v.X, v.Y);
        }
        public static Vector2 ToXNA(this System.Numerics.Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static System.Numerics.Vector3 ToNumerics(this Vector3 v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }
        public static Vector3 ToXNA(this System.Numerics.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static System.Numerics.Vector4 ToNumerics(this Vector4 v)
        {
            return new System.Numerics.Vector4(v.X, v.Y, v.Z, v.W);
        }
        public static Vector4 ToXNA(this System.Numerics.Vector4 v)
        {
            return new Vector4(v.X, v.Y, v.Z, v.W);
        }
    }
}
