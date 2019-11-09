using Engine.GUI;
using Engine.Screens;
using Engine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Threading;

namespace Engine
{
    public static class Loop
    {
        /// <summary>
        /// The target application framerate. The game will be updated and rendered at this frequency, whenever possible.
        /// If set to 0 (zero) then there is no target framerate and the game will update as fast as possible.
        /// Defaults to 0 (unlimited).
        /// </summary>
        public static float TargetFramerate
        {
            get
            {
                return _targetFramerate;
            }
            set
            {
                if (value == _targetFramerate)
                    return;

                if (value < 0)
                    value = 0;

                _targetFramerate = value;
                Debug.Trace($"Updated target framerate to {value} {(value == 0 ? "(none)" : "")}");
            }
        }

        /// <summary>
        /// The current update and draw frequency that the application is running at, calculated each frame.
        /// More accurate at lower framerates, less accurate at higher framerates. For a more stable and reliable value,
        /// see <see cref="Framerate"/>.
        /// </summary>
        public static float ImmediateFramerate { get; private set; } = 0f;

        /// <summary>
        /// Gets the current update and draw frequency, calculated once per second.
        /// The framerate is affected by <see cref="TargetFramerate"/> and <see cref="VSyncMode"/> and of course
        /// the actual speed of game updating and rendering.
        /// </summary>
        public static float Framerate { get; private set; } = 0f;

        /// <summary>
        /// If true, then framerate is limited and maintained using a more accurate technique, leading to more
        /// consistent framerates. However, this can be harder on the CPU, leaving less time for other apps.
        /// Default to off (false).
        /// </summary>
        public static bool EnablePrecsionFramerate { get; set; } = false;

        /// <summary>
        /// Gets or sets the wait time, in milliseconds, that the precision framerate mode uses. <see cref="EnablePrecsionFramerate"/>.
        /// This value cannot be set to lower than zero. Defaults to 0.5ms.
        /// </summary>
        public static double PrecisionFramerateWaitMS
        {
            get
            {
                return _precisionWait;
            }
            set
            {
                if (value > 0.0)
                    _precisionWait = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical sync mode for the display. Default is disabled.
        /// </summary>
        public static VSyncMode VSyncMode
        {
            get
            {
                return vsm;
            }
            set
            {
                if (value == vsm)
                    return;

                vsm = value;
                switch (value)
                {
                    case VSyncMode.DISABLED:
                        JEngine.MainGraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.Immediate;
                        break;
                    case VSyncMode.ENABLED:
                        JEngine.MainGraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.One;
                        break;
                    case VSyncMode.DOUBLE:
                        JEngine.MainGraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.Two;
                        break;
                }

                Debug.Trace($"Updated VSync mode to {value}");
            }
        }

        public static Thread Thread;
        public static bool Running { get; private set; }
        public static bool ThreadQuit { get; private set; }
        public static bool InUIDraw { get; private set; }
        public class Stats
        {
            public double FrameTotalTime;
            public double FrameUpdateTime;
            public double FrameDrawTime;
            public double FrameSleepTime;
            public double FramePresentingTime;
            public bool Waited;
            public int RenderTargetDraws;
            public GraphicsMetrics DrawMetrics { get; internal set; }
        }
        public static Stats Statistics { get; private set; } = new Stats();

        private static float _targetFramerate = 0;
        private static double _precisionWait = 0.5;
        private static int cumulativeFrames;
        private static Stopwatch frameTimer = new Stopwatch();
        private static VSyncMode vsm = VSyncMode.ENABLED;

        private static double TargetFramerateInterval()
        {
            // Remember physics: f=1/i  so  i=1/f
            return 1.0 / TargetFramerate;
        }

        public static void Start()
        {
            if (Running)
                return;

            Running = true;
            ThreadQuit = false;

            Thread = new Thread(Run);
            Thread.Name = "Game Loop";
            Thread.Priority = ThreadPriority.Highest;

            Thread.Start();
            frameTimer.Start();
            Framerate = 0;
            ImmediateFramerate = 0;
        }

        public static void Stop()
        {
            if (!Running)
                return;

            Running = false;
        }

        public static void StopAndWait()
        {
            Stop();
            while (!ThreadQuit)
            {
                Thread.Sleep(1);
            }
        }

        private static void Run()
        {
            Begin();

            Debug.Trace("Starting game loop...");
            SpriteBatch spr = JEngine.MainSpriteBatch;
            Stopwatch watch = new Stopwatch();
            Stopwatch watch2 = new Stopwatch();
            Stopwatch watch3 = new Stopwatch();
            Stopwatch sleepWatch = new Stopwatch();

            double updateTime = 0.0;
            double renderTime = 0.0;
            double presentTime = 0.0;
            double total = 0.0;
            double sleep = 0.0;

            while (Running)
            {
                watch2.Restart();

                // Determine the ideal loop time, in seconds.
                double target = 0.0;
                if (TargetFramerate != 0f)
                    target = TargetFramerateInterval();

                Time.SelfUpdate();

                watch.Restart();
                Update();
                watch.Stop();
                updateTime = watch.Elapsed.TotalSeconds;
                Statistics.FrameUpdateTime = updateTime;

                watch.Restart();
                Draw(spr);
                watch.Stop();
                renderTime = watch.Elapsed.TotalSeconds;
                Statistics.FrameDrawTime = renderTime;

                watch.Restart();
                Present();
                watch.Stop();
                presentTime = watch.Elapsed.TotalSeconds;
                Statistics.FramePresentingTime = presentTime;

                total = updateTime + renderTime + presentTime;
                sleep = target - total;

                if (sleep > 0.0)
                {
                    sleepWatch.Restart();
                    if (!EnablePrecsionFramerate)
                    {
                        // Sleep using the normal method. Allow the CPU to do whatever it wants.
                        TimeSpan s = TimeSpan.FromSeconds(sleep);
                        Thread.Sleep(s);
                    }
                    else
                    {
                        // Sleep by slowly creeping up to the target time in a loop.
                        watch3.Restart();
                        TimeSpan ts = TimeSpan.FromMilliseconds(0.1);
                        while (watch3.Elapsed.TotalSeconds + (0.001) < sleep)
                        {
                            Thread.Sleep(ts);
                        }
                        watch3.Stop();
                    }
                    sleepWatch.Stop();
                    Statistics.FrameSleepTime = sleepWatch.Elapsed.TotalSeconds;
                    Statistics.Waited = true;
                }
                else
                {
                    Statistics.Waited = false;
                }

                watch2.Stop();
                ImmediateFramerate = (float)(1.0 / watch2.Elapsed.TotalSeconds);
                Statistics.FrameTotalTime = watch2.Elapsed.TotalSeconds;
            }

            ThreadQuit = true;
            Thread = null;
            Debug.Trace("Stopped game loop!");
        }

        /// <summary>
        /// Called once when the game boots up.
        /// </summary>
        private static void Begin()
        {

        }

        private static void Update()
        {
            cumulativeFrames++;
            if (frameTimer.Elapsed.TotalSeconds >= 1.0)
            {
                frameTimer.Restart();
                Framerate = cumulativeFrames;
                cumulativeFrames = 0;
            }

            Input.StartFrame();
            Debug.Update();

            #region Debug Text
            Debug.Text($"FPS: {Framerate:F0} (Target: {(TargetFramerate == 0 ? "uncapped" : TargetFramerate.ToString("F0"))}, VSync: {VSyncMode})");
            Debug.Text($"Time Scale: {Time.TimeScale}");
            Debug.Text($"Screen Res: ({Screen.Width}x{Screen.Height})");
            Debug.Text($"Allocated memory: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB.");
            Debug.Text($"Texture Swap Count: {Loop.Statistics.DrawMetrics.TextureCount}");
            Debug.Text($"Draw Calls: {Loop.Statistics.DrawMetrics.DrawCount}");
            Debug.Text($"Sprites Drawn: {Loop.Statistics.DrawMetrics.SpriteCount}");
            Debug.Text($"Render Target Draw Count: {Loop.Statistics.RenderTargetDraws}");
            Debug.Text($"Total Entities: {JEngine.Entities.EntityCount} of {JEngine.Entities.MaxEntityCount}.");
            var tm = JEngine.TileMap;
            var tilePos = tm.PixelToTileCoords((int)Input.MouseWorldPos.X, (int)Input.MouseWorldPos.Y);
            var chunkPos = tm.TileToChunkCoords(tilePos.X, tilePos.Y);
            Chunk chunk = tm.GetChunk(chunkPos);
            Debug.Box(new Rectangle(chunkPos.X * Chunk.SIZE * Tile.SIZE, chunkPos.Y * Chunk.SIZE * Tile.SIZE, Chunk.SIZE * Tile.SIZE, Chunk.SIZE * Tile.SIZE), new Color(20, 60, 160, 50));
            if (chunk == null)
            {
                Debug.Text($"Chunk under mouse ({chunkPos}) is not loaded.");
            }
            else
            {
                Debug.Text($"Chunk under mouse: {chunkPos}, contains {chunk.EntityCount} entities.");
            }
            #endregion

            if (Input.KeyDown(Keys.E))
                JEngine.Camera.UpdateViewBounds = !JEngine.Camera.UpdateViewBounds;

            // Update currently active screen.
            JEngine.EngineUpdate();
        }

        private static void Draw(SpriteBatch spr)
        {
            JEngine.Camera.UpdateMatrix(JEngine.MainGraphicsDevice);
            JEngine.MainGraphicsDevice.SetRenderTarget(null);
            JEngine.MainGraphicsDevice.Clear(JEngine.BackgroundColor);

            SamplerState s = JEngine.Camera.Zoom > 1 ? SamplerState.PointClamp : SamplerState.LinearClamp;
            spr.Begin(SpriteSortMode.Deferred, null, s, null, null, null, JEngine.Camera.GetMatrix());

            // Draw the world.
            JEngine.EngineDraw();
            Debug.Draw(spr);

            spr.End();

            InUIDraw = true;
            spr.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);

            // Draw the UI.
            UILayout.FrameReset();
            JEngine.EngineDrawUI();
            Debug.DrawUI(spr);

            spr.End();
            InUIDraw = false;

            Statistics.RenderTargetDraws = GameScreen.RTDrawCount;
            GameScreen.RTDrawCount = 0;
        }

        private static void Present()
        {
            Statistics.DrawMetrics = JEngine.MainGraphicsDevice.Metrics;
            JEngine.MainGraphicsDevice.Present();
        }
    }
}
