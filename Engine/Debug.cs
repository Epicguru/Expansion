﻿using Engine.IO;
using Engine.Screens;
using Engine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public static bool LogCallingMethod { get; set; } = true;
        public static bool LogEnableTrace { get; set; } = true;
        public static bool LogEnableSave { get; set; } = true;
        public static bool Highlight { get; set; } = false;

        public static bool LogPrintLevel { get; set; } = true;
        public static bool LogPrintTime { get; set; } = false;
        public static string LogFilePath { get; private set; }

        internal static readonly List<string> DebugTexts = new List<string>();
        private static StringBuilder StringBuilder = new StringBuilder();
        private static StringBuilder TextBuilder = new StringBuilder();
        private static List<string> LogEntries = new List<string>();
        private static bool DoneInit = false;
        private static readonly object Key = new object();
        private static Dictionary<string, System.Diagnostics.Stopwatch> Timers = new Dictionary<string, System.Diagnostics.Stopwatch>();
        private static Queue<System.Diagnostics.Stopwatch> TimerPool = new Queue<System.Diagnostics.Stopwatch>();
        private static List<string> TimerNames = new List<string>();
        private static List<(DrawInstruction, object[] args)> toDraw = new List<(DrawInstruction, object[] args)>();
        private static List<(DrawInstruction, object[] args)> toDrawUI = new List<(DrawInstruction, object[] args)>();

        public delegate void DrawInstruction(SpriteBatch spr, object[] args);

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
            TimeSpan s = StopInternal(key, log, out bool stopped);
            if (stopped)
            {
                TimerNames.Remove(key);
            }
            return s;
        }

        public static TimeSpan StopTimer(bool log = false)
        {
            string name = TimerNames.Count == 0 ? null : TimerNames[TimerNames.Count - 1];
            if (TimerNames.Count != 0)
                TimerNames.RemoveAt(TimerNames.Count - 1);

            if (name == null)
                Warn("No timers are currently running, cannot stop.");

            return StopInternal(name, log, out bool stopped);
        }

        private static TimeSpan StopInternal(string key, bool log, out bool didStop)
        {
            if (string.IsNullOrEmpty(key))
            {
                didStop = false;
                return TimeSpan.Zero;
            }

            if (!Timers.ContainsKey(key))
            {
                Warn($"Did not find timer for key '{key}'. Returning zero time span.");
                didStop = false;
                return TimeSpan.Zero;
            }

            var sw = Timers[key];
            sw.Stop();
            Timers.Remove(key);

            if (log)
            {
                Trace($"{key}: {sw.Elapsed.TotalMilliseconds:F2} ms");
            }

            didStop = true;
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
            if(s != null)
                DebugTexts.Add(s);
        }

        public static void TextAt(string text, Vector2 position, Color color)
        {
            if (Loop.InUIDraw)
            {
                toDrawUI.Add((DrawText, new object[] { text, position, color }));
            }
            else
            {
                toDraw.Add((DrawText, new object[] { text, position, color }));
            }
        }

        private static void DrawText(SpriteBatch spr, object[] args)
        {
            spr.DrawString(LoadingScreen.font, (string)args[0], (Vector2)args[1], (Color)args[2]);
        }

        public static void Box(Rectangle position, Color color)
        {
            if (Loop.InUIDraw)
            {
                toDrawUI.Add((DrawRect, new object[] { position, color }));
            }
            else
            {
                toDraw.Add((DrawRect, new object[] { position, color }));
            }
        }

        private static void DrawRect(SpriteBatch spr, object[] args)
        {
            Rectangle bounds = (Rectangle)args[0];
            Color c = (Color)args[1];

            spr.Draw(JEngine.Pixel, bounds, c);
        }

        internal static void Update()
        {
            DebugTexts.Clear();
        }

        internal static void Draw(SpriteBatch spr)
        {
            bool vis = JEngine.ScreenManager.GetScreen<DebugDisplayScreen>().Visible;
            if(!vis)
            {
                toDraw.Clear();
                return;
            }
            // Draw all stuff that is pending.
            while (toDraw.Count > 0)
            {
                (var method, object[] args) = toDraw[0];
                toDraw.RemoveAt(0);

                method.Invoke(spr, args);
            }
        }

        internal static void DrawUI(SpriteBatch spr)
        {
            bool vis = JEngine.ScreenManager.GetScreen<DebugDisplayScreen>().Visible;
            if (!vis)
            {
                toDrawUI.Clear();
                return;
            }

            double total = Loop.Statistics.FrameTotalTime;
            double update = Loop.Statistics.FrameUpdateTime;
            double draw = Loop.Statistics.FrameDrawTime;
            double present = Loop.Statistics.FramePresentingTime;
            double sleep = Loop.Statistics.FrameSleepTime;
            double other = total - (update + draw + present + sleep);

            bool waited = Loop.Statistics.Waited;
            if (total == 0.0)
                total = 0.1;

            int i = 0;
            DrawPart(i++, "Update", update, total, Color.Violet);
            DrawPart(i++, "Draw", draw, total, Color.LightSeaGreen);
            DrawPart(i++, "Present", present, total, Color.IndianRed);
            DrawPart(i++, $"Sleep ({(waited ? 'Y' : 'N')})", sleep, total, Color.Khaki);
            DrawPart(i++, "Other", other, total, Color.Beige);

            // Draw all UI stuff that is pending.
            while(toDrawUI.Count > 0)
            {
                (var method, object[] args) = toDrawUI[0];
                toDrawUI.RemoveAt(0);

                method.Invoke(spr, args);
            }
        }

        private static StringBuilder str = new StringBuilder();
        private static void DrawPart(int index, string name, double time, double total, Color c)
        {
            const int WIDTH = 200;
            const int HEIGHT = 24;
            int x = Screen.Width - WIDTH - 5;
            int y = 5 + index * (HEIGHT + 2);

            double p = time / total;

            str.Clear();
            str.Append(name);
            str.Append(" [");
            str.Append((time * 1000).ToString("F1"));
            str.Append("ms, ");
            str.Append((p * 100.0).ToString("F1"));
            str.Append("%]");

            Debug.Box(new Rectangle(x, y, WIDTH, HEIGHT), Color.DimGray);
            Debug.Box(new Rectangle(x, y, (int)Math.Round(p * WIDTH), HEIGHT), c);
            Debug.TextAt(str.ToString(), new Vector2(x + 3, y + 2), Color.Black);
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

            string stack = Environment.StackTrace;

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

            TextBuilder.Append(text ?? "<null>");
            StringBuilder.Append(text ?? "<null>");

            StringBuilder.AppendLine();
            StringBuilder.Append(stack);            

            if(level != LogLevel.TRACE || (LogEnableTrace))
            {
                if (Console.ForegroundColor != colour)
                    Console.ForegroundColor = colour;
                if (Highlight)
                {
                    switch (level)
                    {
                        case LogLevel.INFO:
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                            break;
                        case LogLevel.WARN:
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                            break;
                        case LogLevel.ERROR:
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            break;
                    }
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                Console.WriteLine(TextBuilder.ToString());
                Console.BackgroundColor = ConsoleColor.Black;
            }

            LogEntries.Add(StringBuilder.ToString());
            StringBuilder.Clear();
            TextBuilder.Clear();

            if (LogCallingMethod)
            {
                string[] split = stack.Split('\n');
                string interesting = split[4];
                string[] inSplit = interesting.Split('(');
                string methodName = inSplit[0].Substring(6);
                string fullPath = interesting.Substring(interesting.LastIndexOf(')'));
                fullPath = fullPath.Substring(fullPath.LastIndexOf('\\') + 1);

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"   from [{methodName.Trim()} -> {fullPath.Trim()}]");
            }            
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
