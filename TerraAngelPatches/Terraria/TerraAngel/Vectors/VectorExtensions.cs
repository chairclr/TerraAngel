using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel.Vectors;

public static class TAVectorExtensions
{
    public static Vector3 XYZ(this Vector4 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
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
