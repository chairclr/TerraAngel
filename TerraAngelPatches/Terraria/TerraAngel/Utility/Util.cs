using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
            screenPoint.X += Main.mapFullscreenPos.X * Main.mapFullscreenScale;
            screenPoint.Y += Main.mapFullscreenPos.Y * Main.mapFullscreenScale;
            screenPoint.X -= Main.screenWidth / 2;
            screenPoint.Y -= Main.screenHeight / 2;
            float xScaled = (screenPoint.X / Main.mapFullscreenScale) * 16;
            float yScaled = (screenPoint.Y / Main.mapFullscreenScale) * 16;
            return new Vector2(xScaled, yScaled);
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
        public static Vector2 WorldToScreenExact(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition.Floor() - Main.screenPosition.Floor(), Main.GameViewMatrix.ZoomMatrix).Floor();
        }
        public static Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(Main.GameViewMatrix.ZoomMatrix)) + Main.screenPosition;
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
    }
}
