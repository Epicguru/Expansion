using Engine.Screens;
using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.GUI
{
    public static class UI
    {
        private static SpriteBatch SB { get { return JEngine.MainSpriteBatch; } }
        private static SpriteFont CurrentFont { get { return LoadingScreen.font; } } // TODO fixme, load fonts.

        public static Rectangle DrawButton(string text, Vector2 pos, Color color, out bool clicked)
        {
            Vector2 size = GetTextSize(CurrentFont, text);
            NinePatch np = LoadingScreen.buttonNP;

            int paddingLeft = -4;
            int paddingRight = -4;
            int paddingAbove = -3;
            int paddingBelow = -5;

            Rectangle bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)Math.Ceiling(size.X) + np.MinWidth + paddingLeft + paddingRight, (int)Math.Ceiling(size.Y) + np.MinHeight + paddingAbove + paddingBelow);

            LoadingScreen.buttonNP.Draw(SB, bounds, color); // TODO fixme, load actual nine patch.
            var textRect = DrawLabel(text, (bounds.Location + new Point(np.LeftSize + paddingLeft, np.TopSize + paddingAbove)).ToVector2(), Color.Black, 0f);

            bool mouseDown = Input.LeftMouseDown();
            clicked = mouseDown && bounds.Contains(Input.MousePos);

            return bounds;
        }

        public static Rectangle DrawLabel(string text, Vector2 pos, Color color, float rotation)
        {
            SB.DrawString(CurrentFont, text ?? "null", pos, color, rotation, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            return GetTextBounds(CurrentFont, pos, text);
        }

        private static Vector2 GetTextSize(SpriteFont font, string s)
        {
            Vector2 size = font.MeasureString(s);
            return size;
        }

        private static Rectangle GetTextBounds(SpriteFont font, Vector2 pos, string s)
        {
            return new Rectangle(pos.ToPoint(), GetTextSize(font, s).ToPoint());
        }
    }
}
