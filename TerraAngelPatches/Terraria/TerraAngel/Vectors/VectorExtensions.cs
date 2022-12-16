using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel.Vectors;

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

    public static Vector2 Lerp(Vector2 x0, Vector2 x1, float t)
    {
        return new Vector2(Util.Lerp(x0.X, x1.X, t), Util.Lerp(x0.Y, x1.Y, t));
    }
    public static Vector2 Lerp(Vector2 x0, Vector2 x1, Vector2 t)
    {
        return new Vector2(Util.Lerp(x0.X, x1.X, t.X), Util.Lerp(x0.Y, x1.Y, t.Y));
    }

    public static Vector2 Ceiling(this Vector2 vec)
    {
        vec.X = MathF.Ceiling(vec.X);
        vec.Y = MathF.Ceiling(vec.Y);
        return vec;
    }
}
