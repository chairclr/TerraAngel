using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

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

        public TimeSpan Sum { get; private set; }
        public TimeSpan Average => Sum / Times.Count;

        public float LerpValue;

        public void Add(TimeSpan time)
        {
            Times.Enqueue(time);

            Sum += time;

            if (Times.Count > 100) Sum -= Times.Dequeue();
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