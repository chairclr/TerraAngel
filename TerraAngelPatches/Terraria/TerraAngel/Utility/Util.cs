using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TerraAngel.Utility;

public class Util
{
    private static readonly string[] ByteSizeNames = { "b", "k", "m", "g", "t", "p" };

    public static readonly Dictionary<int, FieldInfo> ItemFields =
        GetPublicValueTypeFields<ItemID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static readonly Dictionary<int, FieldInfo> TileFields =
        GetPublicValueTypeFields<TileID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static readonly Dictionary<int, FieldInfo> WallFields =
        GetPublicValueTypeFields<WallID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static readonly Dictionary<int, FieldInfo> PrefixFields =
        GetPublicValueTypeFields<PrefixID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static readonly Dictionary<int, FieldInfo> NPCFields =
        GetPublicValueTypeFields<NPCID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static readonly Dictionary<int, FieldInfo> ProjectileFields =
        GetPublicValueTypeFields<ProjectileID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static readonly Dictionary<int, FieldInfo> AmmoFields =
        typeof(AmmoID).GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.FieldType.IsValueType)
        .ToDictionary(x => UnboxStaticFieldToInt(x), x => x);

    public static readonly Dictionary<int, FieldInfo> BuffFields =
        GetPublicValueTypeFields<BuffID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static readonly Dictionary<int, FieldInfo> MessageFields =
        GetPublicValueTypeFields<MessageID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static Vector2 ScreenSize => ClientLoader.WindowManager?.Size ?? Vector2.One;

    public static IEnumerable<FieldInfo> GetPublicValueTypeFields<T>()
    {
        return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.FieldType.IsValueType && x.IsLiteral && !x.IsInitOnly);
    }
    public static int UnboxRawFieldToInt(FieldInfo field)
    {
        dynamic? dyn = field.GetRawConstantValue();

        if (dyn is null)
            return 0;
        return (int)dyn;
    }
    public static int UnboxStaticFieldToInt(FieldInfo field)
    {
        dynamic? dyn = field.GetValue(null);

        if (dyn is null)
            return 0;
        return (int)dyn;
    }

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
}