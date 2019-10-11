
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

        public static float deltaTime { get; private set; }
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
