using System;
namespace TerraAngel.Utility;

public static class ColorExtensions
{
    public static int SqrDistance(this Color x, Color y)
    {
        return SqrColorDistance(x, y);
    }

    public static float Distance(this Color x, Color y)
    {
        return ColorDistance(x, y);
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
}