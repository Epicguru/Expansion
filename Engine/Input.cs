using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// A static class that collects all of XNA's input utilities into one place.
    /// Mouse, Keyboard and (possibly in the future) Gamepad inputs are all available here.
    /// Input is polled and updated on a per-frame basis, so input state will not change in the middle of
    /// a frame execution. Has various extra features, such as disabling input when necessary, and detecting when the
    /// mouse exits the game window.
    /// </summary>
    public static class Input
    {
        public static bool Enabled { get; set; } = true;

        public static Point MousePos { get; private set; }
        public static Point MouseWorldTilePos { get; private set; }
        public static Vector2 MouseWorldPos { get; private set; }
        public static bool IsMouseInWindow { get; private set; }
        public static int MouseScroll { get; private set; }
        public static int MouseScrollDelta { get; private set; }

        private static KeyboardState CurrentKeyState;
        private static KeyboardState LastKeyState;

        private static MouseState CurrentMouseState;
        private static MouseState LastMouseState;

        public static void StartFrame()
        {
            LastKeyState = CurrentKeyState;
            CurrentKeyState = Keyboard.GetState();

            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            MousePos = CurrentMouseState.Position;
            MouseWorldPos = JEngine.Camera.ScreenToWorldPosition(MousePos.ToVector2());
            IsMouseInWindow = Screen.Contains((int)MousePos.X, (int)MousePos.Y);
            if (JEngine.TileMap != null)
                MouseWorldTilePos = JEngine.TileMap.PixelToTileCoords((int)MouseWorldPos.X, (int)MouseWorldPos.Y);

            MouseScrollDelta = CurrentMouseState.ScrollWheelValue - MouseScroll;
            MouseScroll = CurrentMouseState.ScrollWheelValue;
        }

        public static bool KeyPressed(Keys key)
        {
            return Enabled && Pressed(CurrentKeyState[key]);
        }

        public static bool KeyDown(Keys key)
        {
            return Enabled && Down(CurrentKeyState[key], LastKeyState[key]);
        }

        public static bool KeyUp(Keys key)
        {
            return Enabled && Up(CurrentKeyState[key], LastKeyState[key]);
        }

        private static bool MousePressed(ButtonState s)
        {
            return Enabled && s == ButtonState.Pressed;
        }

        private static bool MouseDown(ButtonState current, ButtonState last)
        {
            return Enabled && current == ButtonState.Pressed && last == ButtonState.Released;
        }

        private static bool MouseUp(ButtonState current, ButtonState last)
        {
            return Enabled && current == ButtonState.Released && last == ButtonState.Pressed;
        }

        public static bool RightMousePressed()
        {
            return MousePressed(CurrentMouseState.RightButton);
        }

        public static bool LeftMousePressed()
        {
            return MousePressed(CurrentMouseState.LeftButton);
        }

        public static bool MiddleMousePressed()
        {
            return MousePressed(CurrentMouseState.MiddleButton);
        }

        public static bool RightMouseDown()
        {
            return MouseDown(CurrentMouseState.RightButton, LastMouseState.RightButton);
        }

        public static bool LeftMouseDown()
        {
            return MouseDown(CurrentMouseState.LeftButton, LastMouseState.LeftButton);
        }

        public static bool MiddleMouseDown()
        {
            return MouseDown(CurrentMouseState.MiddleButton, LastMouseState.MiddleButton);
        }

        public static bool RightMouseUp()
        {
            return MouseUp(CurrentMouseState.RightButton, LastMouseState.RightButton);
        }

        public static bool LeftMouseUp()
        {
            return MouseUp(CurrentMouseState.LeftButton, LastMouseState.LeftButton);
        }

        public static bool MiddleMouseUp()
        {
            return MouseUp(CurrentMouseState.MiddleButton, LastMouseState.MiddleButton);
        }

        private static bool Pressed(KeyState s)
        {
            return s == KeyState.Down;
        }

        private static bool Down(KeyState current, KeyState last)
        {
            return current == KeyState.Down && last == KeyState.Up;
        }

        private static bool Up(KeyState current, KeyState last)
        {
            return current == KeyState.Up && last == KeyState.Down;
        }
    }
}
