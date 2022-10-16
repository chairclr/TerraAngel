using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TerraAngel.Utility;

public class Util
{
    private static string[] ByteSizeNames = { "b", "k", "m", "g", "t", "p" };

    public static Dictionary<int, FieldInfo> ItemFields =
        GetPublicValueTypeFields<ItemID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static Dictionary<int, FieldInfo> TileFields =
        GetPublicValueTypeFields<TileID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static Dictionary<int, FieldInfo> WallFields =
        GetPublicValueTypeFields<WallID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static Dictionary<int, FieldInfo> PrefixFields =
        GetPublicValueTypeFields<PrefixID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static Dictionary<int, FieldInfo> NPCFields =
        GetPublicValueTypeFields<NPCID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static Dictionary<int, FieldInfo> ProjectileFields =
        GetPublicValueTypeFields<ProjectileID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static Dictionary<int, FieldInfo> AmmoFields =
        typeof(AmmoID).GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.FieldType.IsValueType)
        .ToDictionary(x => UnboxStaticFieldToInt(x), x => x);

    public static Dictionary<int, FieldInfo> BuffFields =
        GetPublicValueTypeFields<BuffID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static Dictionary<int, FieldInfo> MessageFields =
        GetPublicValueTypeFields<MessageID>()
        .ToDictionary(x => UnboxRawFieldToInt(x), x => x);

    public static Vector2 ScreenSize => new Vector2((float)Main.screenWidth, (float)Main.screenHeight);


    public static IEnumerable<FieldInfo> GetPublicValueTypeFields<T>()
    {
        return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.FieldType.IsValueType);
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
    public static Vector2 WorldToScreenDynamicExact(Vector2 worldPoint)
    {
        if (Main.mapFullscreen) return WorldToScreenFullscreenMap(worldPoint);
        else return WorldToScreenWorldExact(worldPoint);
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
    public static Vector2 WorldToScreenWorldExact(Vector2 worldPosition)
    {
        return Vector2.Transform((worldPosition - Main.screenPosition).Floor(), Main.GameViewMatrix.ZoomMatrix).Floor();
    }
    public static Vector2 ScreenToWorldWorld(Vector2 screenPosition)
    {
        return Vector2.Transform(screenPosition, Main.GameViewMatrix.InverseZoomMatrix) + Main.screenPosition;
    }

    public static void CreateDirectory(string dir)
    {
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
    public static void CreateParentDirectory(string path) => CreateDirectory(Path.GetDirectoryName(path));

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

    public static int SqrColorDistance(Color x, Color y)
    {
        int difr = x.R - y.R;
        int difg = x.G - y.G;
        int difb = x.B - y.B;

        return difr * difr + difg * difg + difb * difb;
    }

    public static float ColorDistance(Color col1, Color col2)
    {
        return MathF.Sqrt(SqrColorDistance(col1, col2));
    }

    public class Map<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    {
        private readonly Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
        private readonly Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

        public Map()
        {
            Forward = new Indexer<T1, T2>(_forward);
            Reverse = new Indexer<T2, T1>(_reverse);
        }

        public Indexer<T1, T2> Forward { get; private set; }
        public Indexer<T2, T1> Reverse { get; private set; }

        public void Add(T1 t1, T2 t2)
        {
            _forward.Add(t1, t2);
            _reverse.Add(t2, t1);
        }

        public void Remove(T1 t1)
        {
            T2 revKey = Forward[t1];
            _forward.Remove(t1);
            _reverse.Remove(revKey);
        }

        public void Remove(T2 t2)
        {
            T1 forwardKey = Reverse[t2];
            _reverse.Remove(t2);
            _forward.Remove(forwardKey);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return _forward.GetEnumerator();
        }

        public class Indexer<T3, T4>
        {
            private readonly Dictionary<T3, T4> _dictionary;

            public Indexer(Dictionary<T3, T4> dictionary)
            {
                _dictionary = dictionary;
            }

            public T4 this[T3 index]
            {
                get { return _dictionary[index]; }
                set { _dictionary[index] = value; }
            }

            public bool Contains(T3 key)
            {
                return _dictionary.ContainsKey(key);
            }
        }
    }



    public static object GetDefault(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }

    public static string EscapeString(string value)
    {
        try
        {
            value = Regex.Unescape(value);
        }
        catch (RegexParseException)
        {

        }
        return value;
    }
    public static int CompareStringDist(string s, string t)
    {
        if (string.IsNullOrEmpty(t))
        {
            throw new ArgumentNullException(t, "String Cannot Be Null Or Empty");
        }

        int n = s.Length;
        int m = t.Length;

        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        int[] p = new int[n + 1];
        int[] d = new int[n + 1];

        int i;
        int j;

        for (i = 0; i <= n; i++)
        {
            p[i] = i;
        }

        for (j = 1; j <= m; j++)
        {
            char tJ = t[j - 1];
            d[0] = j;

            for (i = 1; i <= n; i++)
            {
                int cost = s[i - 1] == tJ ? 0 : 1;

                d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
            }

            int[] dPlaceholder = p; //placeholder to assist in swapping p and d
            p = d;
            d = dPlaceholder;

        }

        // our last action in the above loop was to switch d and p, so p now 
        // actually has the most recent cost counts
        return p[n];
    }

}
public static class VectorExtensions
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

    public static System.Numerics.Vector3 XYZ(this System.Numerics.Vector4 v)
    {
        return new System.Numerics.Vector3(v.X, v.Y, v.Z);
    }
    public static Vector2 Round(this Vector2 vec)
    {
        return new Vector2(MathF.Round(vec.X), MathF.Round(vec.Y));
    }
    public static Vector2 Normalized(this Vector2 vec)
    {
        vec.Normalize();
        return vec;
    }
}
public static class ColorExtensions
{
    public static int SqrDistance(this Color x, Color y)
    {
        return Util.SqrColorDistance(x, y);
    }

    public static float Distance(this Color x, Color y)
    {
        return Util.ColorDistance(x, y);
    }

    public static Color WithAlpha(this Color x, float alpha)
    {
        return new Color(x.R, x.G, x.B, alpha);
    }
    public static Color WithRed(this Color x, float r)
    {
        return new Color((int)(r * 255f), x.G, x.B, x.A);
    }
    public static Color WithGreen(this Color x, float g)
    {
        return new Color(x.R, (int)(g * 255f), x.B, x.A);
    }
    public static Color WithBlue(this Color x, float b)
    {
        return new Color(x.R, x.G, (int)(b * 255f), x.A);
    }
}
public static class ListExtensions
{
    public static string StringSum<T>(this List<T> e, Func<T, string> predicate)
    {
        string s = "";
        e.ForEach((x) => s += predicate(x));
        return s;
    }
}
public static class StringExtensions
{
    public static string Truncate(this string value, int maxChars)
    {
        return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
    }
}