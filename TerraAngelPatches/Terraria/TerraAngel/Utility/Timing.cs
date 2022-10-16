using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TerraAngel.Utility;
public class TimeMetrics
{
    public abstract class Timer
    {
        public readonly string Name;
        public readonly Stopwatch Watch;

        public Timer(string name)
        {
            Name = name;
            Watch = new Stopwatch();
        }
    }

    public class TimeLog
    {
        public Queue<TimeSpan> Times = new Queue<TimeSpan>();
        public int MaxTimeCount = 100;

        public TimeSpan Sum = TimeSpan.Zero;
        public TimeSpan Average
        {
            get
            {
                if (Times.Count == 0 || Sum == TimeSpan.Zero)
                    return TimeSpan.Zero;
                return Sum / Times.Count;
            }
        }
        public bool Paused = false;

        public float LerpValue;

        public void Add(TimeSpan time)
        {
            if (Paused) return;
            Times.Enqueue(time);

            Sum += time;

            if (Times.Count > MaxTimeCount) Sum -= Times.Dequeue();
        }

        public TimeLog()
        {
            MaxTimeCount = 100;
        }

        public TimeLog(int maxTimeCount)
        {
            MaxTimeCount = maxTimeCount;
        }
    }

    public static Dictionary<string, TimeLog> TimeLogs = new Dictionary<string, TimeLog>();
    private static Dictionary<string, BasicTimer> basicTimers = new Dictionary<string, BasicTimer>();

    public static BasicTimer GetTimer(string name)
    {
        BasicTimer timer;
        if (!basicTimers.ContainsKey(name))
        {
            timer = new BasicTimer(name);
            basicTimers.Add(name, timer);
        }
        else
        {
            timer = basicTimers[name];
        }

        return timer;
    }
    public static void AddTime(string name, TimeSpan time)
    {
        TimeLog log;
        if (!TimeLogs.ContainsKey(name))
        {
            log = new TimeLog();
            TimeLogs.Add(name, log);
        }
        else
        {
            log = TimeLogs[name];
        }

        log.Add(time);
    }


    public static TimeLog FramerateDeltaTimeSlices = new TimeLog(100);
    public static TimeLog UpdateDeltaTimeSlices = new TimeLog(100);
}

public class BasicTimer : TimeMetrics.Timer
{
    public BasicTimer(string name) : base(name)
    {

    }

    public BasicTimer Start()
    {
        Watch.Restart();
        return this;
    }

    public void Stop()
    {
        Watch.Stop();
        TimeMetrics.AddTime(Name, Watch.Elapsed);
    }
}