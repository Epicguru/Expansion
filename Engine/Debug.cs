using Engine.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Engine
{
    public static class Debug
    {
        // TODO add an option (or just replace current system) to write the log entries straight to file
        // async to reduce memory consumption from having all these strings lying around.

        public static bool LogEnabled { get; set; } = true;
        public static uint LogStackTraceDepth { get; set; } = 3;
        public static bool LogEnableTrace { get; set; } = true;
        public static bool LogEnableSave { get; set; } = true;

        public static bool LogPrintLevel { get; set; } = true;
        public static bool LogPrintTime { get; set; } = false;
        public static string LogFilePath { get; private set; }

        private static StringBuilder StringBuilder = new StringBuilder();
        private static StringBuilder TextBuilder = new StringBuilder();
        private static List<string> LogEntries = new List<string>();
        private static bool DoneInit = false;
        private static readonly object Key = new object();
        private static Dictionary<string, System.Diagnostics.Stopwatch> Timers = new Dictionary<string, System.Diagnostics.Stopwatch>();
        private static Queue<System.Diagnostics.Stopwatch> TimerPool = new Queue<System.Diagnostics.Stopwatch>();

        public static void Init()
        {
            if (DoneInit)
                return;

            DoneInit = true;

            string filePath = Path.Combine(GameIO.LogDirectory, "Log File.txt");
            LogFilePath = filePath;
        }

        public static void Shutdown()
        {
            Log($"Saving {LogEntries.Count} log entries to {LogFilePath}.");
            GameIO.EnsureParentDirectory(LogFilePath);
            using(StreamWriter writer = new StreamWriter(File.Create(LogFilePath)))
            {
                foreach (var line in LogEntries)
                {
                    writer.WriteLine(line);
                }
            }
        }

        private static System.Diagnostics.Stopwatch GetPooledTimer()
        {
            if(TimerPool.Count == 0)
            {
                return new System.Diagnostics.Stopwatch();
            }
            else
            {
                return TimerPool.Dequeue();
            }
        }

        private static void ReturnPooledTimer(System.Diagnostics.Stopwatch s)
        {
            s.Stop();
            TimerPool.Enqueue(s);
;        }

        public static System.Diagnostics.Stopwatch StartTimer(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Warn("Timer key cannot be null or empty. Timer will return a time of 0 seconds.");
                return null;
            }

            //lock (Key)
            {
                if (Timers.ContainsKey(key))
                {
                    Warn($"Timer for key '{key}' already exists. Stop that timer first.");
                    return null;
                }

                var timer = GetPooledTimer();
                Timers.Add(key, timer);
                timer.Restart();
                return timer;
            }
        }

        public static TimeSpan StopTimer(string key, bool log = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return TimeSpan.Zero;
            }

            if (!Timers.ContainsKey(key))
            {
                Warn($"Did not find timer for key '{key}'. Returning zero time span.");
                return TimeSpan.Zero;
            }

            var sw = Timers[key];
            sw.Stop();
            Timers.Remove(key);

            if (log)
            {
                Trace($"{key}: {sw.Elapsed.TotalMilliseconds:F2} ms");
            }

            return sw.Elapsed;
        }

        public static void Assert(bool condition, string errorMsg = null)
        {
            if (!condition)
            {
                Error(errorMsg ?? $"Assertion failed!");
            }
        }

        public static void Text(string s)
        {
            // TODO implement me
        }

        #region Logging

        public static void Trace(string text)
        {
            Print(LogLevel.TRACE, text, ConsoleColor.Gray);
        }

        public static void Log(string text)
        {
            Print(LogLevel.INFO, text, ConsoleColor.White);
        }

        public static void Warn(string text)
        {
            Print(LogLevel.WARN, text, ConsoleColor.Yellow);
        }

        public static void Error(string text, Exception e = null)
        {
            if(text != null)
            {
                Print(LogLevel.ERROR, text, ConsoleColor.Red);
            }

            if(e != null)
            {
                Print(LogLevel.ERROR, e.GetType().FullName + ": " + e.Message, ConsoleColor.Red);
                Print(LogLevel.ERROR, e.StackTrace, ConsoleColor.Red);
            }
        }

        private static void Print(LogLevel level, string text, ConsoleColor colour = ConsoleColor.White)
        {
            if (!LogEnabled)
                return;

            string lvl = null;
            switch (level)
            {
                case LogLevel.TRACE:
                    lvl = "[TRACE] ";
                    break;
                case LogLevel.INFO:
                    lvl = "[INFO] ";
                    break;
                case LogLevel.WARN:
                    lvl = "[WARN] ";
                    break;
                case LogLevel.ERROR:
                    lvl = "[ERROR] ";
                    break;
            }
            lvl = lvl.PadRight(8);
            StringBuilder.Append(lvl);
            if (LogPrintLevel)
                TextBuilder.Append(lvl);

            DateTime t = DateTime.Now;
            string time = $"[{t.Hour}:{t.Minute}:{t.Second}.{t.Millisecond / 10}] ";
            time = time.PadRight(15);
            StringBuilder.Append(time);
            if (LogPrintTime)
                TextBuilder.Append(time);

            TextBuilder.Append(text ?? "null");
            StringBuilder.Append(text ?? "null");

            if(LogStackTraceDepth != 0)
            {
                string stack = Environment.StackTrace;
                StringBuilder.AppendLine();
                StringBuilder.Append(stack);
            }

            if(level != LogLevel.TRACE || (LogEnableTrace))
            {
                if (Console.ForegroundColor != colour)
                    Console.ForegroundColor = colour;

                Console.WriteLine(TextBuilder.ToString());
            }

            LogEntries.Add(StringBuilder.ToString());
            StringBuilder.Clear();
            TextBuilder.Clear();
        }

        #endregion
    }

    public enum LogLevel
    {
        TRACE,
        INFO,
        WARN,
        ERROR
    }
}
