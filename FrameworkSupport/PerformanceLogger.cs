using System;
using System.Diagnostics;

namespace FrameworkSupport
{
    /// <summary>
    /// Outputs a string with a name and elapsed time since last logged message to your specified function
    /// </summary>
    /// <remarks>
    /// Example:
    ///     PerformanceLogger pl = new PerformanceLogger("BulkPrint", true, logger.Debug);
    ///     pl.Log("step 1");
    ///     pl.Log("step 2");
    ///
    /// Output:
    ///     BulkPrint [-]: step 1
    ///     BulkPrint [234 ms]: step 2
    /// </remarks>
    public class PerformanceLogger
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public Action<string> Logger { get; set; }

        private Stopwatch Timer { get; set; }

        public PerformanceLogger(string name, bool enabled, Action<string> logger)
        {
            Name = name;
            Enabled = enabled;
            Logger = logger;
        }

        public void Log(string msg)
        {
            if (!Enabled)
            {
                return;
            }

            string elapsedSinceLastLog = "-";
            if (Timer == null)
            {
                Timer = new Stopwatch();
            }
            else
            {
                Timer.Stop();
                elapsedSinceLastLog = $"{Timer.ElapsedMilliseconds} ms";
                Timer.Reset();
            }
            Logger($"{Name} [{elapsedSinceLastLog}]: {msg}");
            Timer.Start();
        }
    }
}