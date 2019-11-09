
using System.Diagnostics;

namespace Engine
{
    public static class Time
    {
        /// <summary>
        /// If true, the Time class is automatically updated by the engine.
        /// If false, <see cref="Time.Update"/> needs to be called auotmatically from
        /// your app.
        /// </summary>
        public static bool AutoUpdate { get; set; } = true;
        /// <summary>
        /// The global time scale. This affects <see cref="deltaTime"/>. Use this to create slow motion
        /// effects, speed up effect or entirely pause motion by setting to zero. Default is 1, meaining normal time.
        /// </summary>
        public static float TimeScale
        {
            get
            {
                return _timeScale;
            }
            set
            {
                if(value >= 0f)
                {
                    _timeScale = value;
                }                
            }
        }
        /// <summary>
        /// The total elapsed time, in seconds, since the game started.
        /// </summary>
        public static float time { get; private set; }
        /// <summary>
        /// The total elapsed time, in seconds, since the game started. Not affected by time scale.
        /// </summary>
        public static float unscaledTime { get; private set; }

        /// <summary>
        /// The time, in seconds, since the last frame. Multiply this value by another to create a per-second
        /// relationship. Fundamental to create frame-rate independent code.
        /// </summary>
        public static float deltaTime { get; private set; }
        /// <summary>
        /// The same as <see cref="deltaTime"/> but is not affected by the <see cref="TimeScale"/> value.
        /// Useful for UI and other cases where you don't want slow-motion or fast-motion effects to apply.
        /// </summary>
        public static float unscaledDeltaTime { get; private set; }

        private static float _timeScale = 1f;
        private static readonly Stopwatch watch = new Stopwatch();

        /// <summary>
        /// Updates all deltaTime values. Should be called at the very beginning of the frame, before anything
        /// updates. Will autocally be called by the engine if <see cref="AutoUpdate"/> is enabled (by default it is true).
        /// </summary>
        public static void Update()
        {
            watch.Stop();

            unscaledDeltaTime = (float)watch.Elapsed.TotalSeconds;
            deltaTime = unscaledDeltaTime * TimeScale;
            time += deltaTime;
            unscaledTime += unscaledDeltaTime;

            watch.Restart();
        }

        internal static void SelfUpdate()
        {
            if (AutoUpdate)
            {
                Update();
            }
        }
    }
}
