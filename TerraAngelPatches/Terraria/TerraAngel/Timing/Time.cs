using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TerraAngel.Timing;

public class Time
{
    public static float DrawDeltaTime { get; private set; } = 1f / 60f;
    public static float UpdateDeltaTime { get; private set; } = 1f / 60f;

    public static double PerciseDrawDeltaTime { get; private set; } = 1.0 / 60.0;
    public static double PerciseUpdateDeltaTime { get; private set; } = 1.0 / 60.0;

    private static Stopwatch DrawStopwatch = new Stopwatch();
    private static Stopwatch UpdateStopwatch = new Stopwatch();

    public static void UpdateDraw()
    {
        DrawStopwatch.Stop();

        TimeMetrics.FramerateDeltaTimeSlices.Add(DrawStopwatch.Elapsed);
        PerciseDrawDeltaTime = DrawStopwatch.Elapsed.TotalSeconds;
        DrawDeltaTime = (float)PerciseDrawDeltaTime;

        DrawStopwatch.Restart();
    }

    public static void UpdateUpdate()
    {
        UpdateStopwatch.Stop();

        TimeMetrics.UpdateDeltaTimeSlices.Add(UpdateStopwatch.Elapsed);
        PerciseUpdateDeltaTime = UpdateStopwatch.Elapsed.TotalSeconds;
        UpdateDeltaTime = (float)UpdateDeltaTime;

        UpdateStopwatch.Restart();
    }
}
