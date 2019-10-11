using System;

namespace Engine
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                using (var game = new Engine())
                {
                    Engine.Instance = game;
                    game.Run();
                    Engine.Instance = null;
                }
            }
            finally
            {
                Debug.Shutdown();
            }
        }
    }
#endif
}
