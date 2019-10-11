namespace Engine
{
    public static class Screen
    {
        /// <summary>
        /// Current game window width, in pixels.
        /// </summary>
        public static int Width
        {
            get
            {
                return Engine.MainGraphicsDevice.PresentationParameters.BackBufferWidth;
            }
        }

        /// <summary>
        /// Current game window height, in pixels.
        /// </summary>
        public static int Height
        {
            get
            {
                return Engine.MainGraphicsDevice.PresentationParameters.BackBufferHeight;
            }
        }

        /// <summary>
        /// The detected physical monitor width, in pixels. Where there are multiple monitors, the primary one
        /// is normally detected and used.
        /// </summary>
        public static int MonitorWidth
        {
            get
            {
                return Engine.MainGraphicsDevice.DisplayMode.Width;
            }
        }

        /// <summary>
        /// The detected physical monitor height, in pixels. Where there are multiple monitors, the primary one
        /// is normally detected and used.
        /// </summary>
        public static int MonitorHeight
        {
            get
            {
                return Engine.MainGraphicsDevice.DisplayMode.Height;
            }
        }

        /// <summary>
        /// The current vsync mode. Exactly the same as reading and writing <see cref="Loop.VSyncMode"/>.
        /// </summary>
        public static VSyncMode VSyncMode
        {
            get
            {
                return Loop.VSyncMode;
            }
            set
            {
                Loop.VSyncMode = value;
            }
        }

        /// <summary>
        /// Returns true when the point provided is greater than (0, 0) and less that (Width, Height).
        /// </summary>
        public static bool Contains(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }
    }
}
