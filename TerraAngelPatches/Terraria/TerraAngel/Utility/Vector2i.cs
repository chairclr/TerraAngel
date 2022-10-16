using System;

namespace TerraAngel.Utility;

public struct Vector2i
{
    public int X;
    public int Y;

    public Vector2i(int x, int y)
    {
        X = x;
        Y = y;
    }
    public Vector2i(float x, float y)
    {
        X = (int)x;
        Y = (int)y;
    }
    public Vector2i(Vector2 v)
    {
        X = (int)v.X;
        Y = (int)v.Y;
    }
    public Vector2i(Point v)
    {
        X = v.X;
        Y = v.Y;
    }

    public override string ToString()
    {
        return $"{{X: {X}, Y: {Y}}}";
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2i i &&
               X == i.X &&
               Y == i.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static implicit operator Point(Vector2i v)
    {
        return new Point(v.X, v.Y);
    }
    public static implicit operator Vector2(Vector2i v)
    {
        return new Vector2(v.X, v.Y);
    }
    public static implicit operator Vector2i(Point v)
    {
        return new Vector2i(v);
    }
    public static implicit operator Vector2i(Vector2 v)
    {
        return new Vector2i(v);
    }

    public static Vector2i operator *(Vector2i lhs, int rhs)
    {
        return new Vector2i(lhs.X * rhs, lhs.Y * rhs);
    }
    public static Vector2i operator /(Vector2i lhs, int rhs)
    {
        return new Vector2i(lhs.X / rhs, lhs.Y / rhs);
    }
    public static Vector2i operator *(Vector2i lhs, float rhs)
    {
        return new Vector2i(lhs.X * rhs, lhs.Y * rhs);
    }
    public static Vector2i operator /(Vector2i lhs, float rhs)
    {
        return new Vector2i(lhs.X / rhs, lhs.Y / rhs);
    }

    public static Vector2i operator *(int lhs, Vector2i rhs)
    {
        return rhs * lhs;
    }
    public static Vector2i operator /(int lhs, Vector2i rhs)
    {
        return rhs / lhs;
    }
    public static Vector2i operator *(float lhs, Vector2i rhs)
    {
        return rhs * lhs;
    }
    public static Vector2i operator /(float lhs, Vector2i rhs)
    {
        return rhs / lhs;
    }

    public static Vector2i operator +(Vector2i lhs, Vector2i rhs)
    {
        return new Vector2i(lhs.X + rhs.X, lhs.Y + rhs.Y);
    }
    public static Vector2i operator -(Vector2i lhs, Vector2i rhs)
    {
        return new Vector2i(lhs.X - rhs.X, lhs.Y - rhs.Y);
    }
    public static Vector2i operator *(Vector2i lhs, Vector2i rhs)
    {
        return new Vector2i(lhs.X * rhs.X, lhs.Y * rhs.Y);
    }
    public static Vector2i operator /(Vector2i lhs, Vector2i rhs)
    {
        return new Vector2i(lhs.X / rhs.X, lhs.Y / rhs.Y);
    }

    public static Vector2i operator +(Vector2i lhs, Vector2 rhs)
    {
        return new Vector2i(lhs.X + (int)rhs.X, lhs.Y + (int)rhs.Y);
    }
    public static Vector2i operator -(Vector2i lhs, Vector2 rhs)
    {
        return new Vector2i(lhs.X - (int)rhs.X, lhs.Y - (int)rhs.Y);
    }
    public static Vector2i operator *(Vector2i lhs, Vector2 rhs)
    {
        return new Vector2i(lhs.X * (int)rhs.X, lhs.Y * (int)rhs.Y);
    }
    public static Vector2i operator /(Vector2i lhs, Vector2 rhs)
    {
        return new Vector2i(lhs.X / (int)rhs.X, lhs.Y / (int)rhs.Y);
    }

    public static Vector2 operator +(Vector2 lhs, Vector2i rhs)
    {
        return new Vector2i((int)lhs.X + rhs.X, (int)lhs.Y + rhs.Y);
    }
    public static Vector2 operator -(Vector2 lhs, Vector2i rhs)
    {
        return new Vector2i((int)lhs.X - rhs.X, (int)lhs.Y - rhs.Y);
    }
    public static Vector2 operator *(Vector2 lhs, Vector2i rhs)
    {
        return new Vector2i((int)lhs.X * rhs.X, (int)lhs.Y * rhs.Y);
    }
    public static Vector2 operator /(Vector2 lhs, Vector2i rhs)
    {
        return new Vector2i((int)lhs.X / rhs.X, (int)lhs.Y / rhs.Y);
    }

    public static bool operator ==(Vector2i lhs, Vector2i rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y;
    }
    public static bool operator !=(Vector2i lhs, Vector2i rhs)
    {
        return lhs.X != rhs.X || lhs.Y != rhs.Y;
    }
}
