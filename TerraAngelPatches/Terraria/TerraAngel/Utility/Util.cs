using System;

namespace TerraAngel.Utility;

public class Util
{
    private static readonly string[] ByteSizeNames = { "b", "k", "m", "g", "t", "p" };

    public static Vector2 ScreenSize => ClientLoader.WindowManager?.Size ?? Vector2.One;

    public static bool IsRectOnScreen(Vector2 min, Vector2 max, Vector2 displaySize)
    {
        return (min.X > 0 || max.X > 0) && (min.X < displaySize.X || max.X < displaySize.X) && (min.Y > 0 || max.Y > 0) && (min.Y < displaySize.Y || max.X < displaySize.Y);
    }

    public static bool IsMouseHoveringRect(Vector2 min, Vector2 max)
    {
        Vector2 mousePos = InputSystem.MousePosition;
        return mousePos.X >= min.X &&
                mousePos.Y >= min.Y &&
                mousePos.X <= max.X &&
                mousePos.Y <= max.Y;
    }

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


    public static Vector2 WorldToScreenDynamic(Vector2 worldPoint)
    {
        if (Main.mapFullscreen) return WorldToScreenFullscreenMap(worldPoint);
        else return WorldToScreenWorld(worldPoint);
    }

    public static Vector2 WorldToScreenDynamicPixelPerfect(Vector2 worldPoint)
    {
        if (Main.mapFullscreen) return WorldToScreenFullscreenMap(worldPoint);
        else return WorldToScreenWorldPixelPerfect(worldPoint);
    }

    public static Vector2 ScreenToWorldDynamic(Vector2 screenPoint)
    {
        if (Main.mapFullscreen) return ScreenToWorldFullscreenMap(screenPoint);
        else return ScreenToWorldWorld(screenPoint);
    }

    public static Vector2 ScreenToWorldFullscreenMap(Vector2 screenPoint)
    {
        screenPoint += Main.mapFullscreenPos * Main.mapFullscreenScale;
        screenPoint -= ScreenSize / 2f;
        screenPoint /= Main.mapFullscreenScale;
        screenPoint *= 16f;
        return screenPoint;
    }
    public static Vector2 WorldToScreenFullscreenMap(Vector2 worldPoint)
    {
        worldPoint *= Main.mapFullscreenScale;
        worldPoint /= 16f;
        worldPoint -= Main.mapFullscreenPos * Main.mapFullscreenScale;
        worldPoint += ScreenSize / 2f;
        return worldPoint;
    }

    public static Vector2 WorldToScreenWorld(Vector2 worldPosition)
    {
        return Vector2.Transform(worldPosition - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);
    }
    public static Vector2 WorldToScreenWorldPixelPerfect(Vector2 worldPosition)
    {
        return Vector2.Transform((worldPosition - Main.screenPosition).Floor(), Main.GameViewMatrix.ZoomMatrix).Floor();
    }
    public static Vector2 ScreenToWorldWorld(Vector2 screenPosition)
    {
        return Vector2.Transform(screenPosition, Main.GameViewMatrix.InverseZoomMatrix) + Main.screenPosition;
    }

    public static object? GetDefault(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }

    public static float Lerp(float x0, float x1, float t)
    {
        return MathHelper.Lerp(x0, x1, t);
    }

    public static void Bresenham(int x, int y, int x2, int y2, Action<int, int> predicate)
    {
        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            predicate(x, y);
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
    }
}