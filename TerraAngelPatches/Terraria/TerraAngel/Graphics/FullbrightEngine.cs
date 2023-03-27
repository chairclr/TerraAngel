using ReLogic.Threading;
using Terraria.Graphics.Light;

namespace TerraAngel.Graphics;

public class FullbrightEngine : ILightingEngine
{
    public static float Brightness = 1.0f;
    public void AddLight(int x, int y, Vector3 color)
    {
    }

    public void Clear()
    {
    }

    public Vector3 GetColor(int x, int y)
    {
        return new Vector3(Brightness);
    }

    static int state = 0;

    public void ProcessArea(Rectangle area)
    {
        Main.renderCount = (Main.renderCount + 1) % 4;
        state = ((state + 1) % 5);
        if (state == 0)
        {
            if (Main.mapDelay > 0)
            {
                Main.mapDelay--;
            }
            else
            {
                Rectangle value = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
                value.Inflate(-40, -40);
                area = Rectangle.Intersect(area, value);
                Main.mapMinX = area.Left;
                Main.mapMinY = area.Top;
                Main.mapMaxX = area.Right;
                Main.mapMaxY = area.Bottom;

                FastParallel.For(area.Left, area.Right, delegate (int start, int end, object context)
                {
                    for (int i = start; i < end; i++)
                    {
                        for (int j = area.Top; j < area.Bottom; j++)
                        {
                            Main.Map.Update(i, j, 255);
                        }
                    }
                });

                Main.updateMap = true;
            }
        }
    }

    public void Rebuild()
    {
    }
}
