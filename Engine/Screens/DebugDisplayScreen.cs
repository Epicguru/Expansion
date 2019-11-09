using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Screens
{
    internal class DebugDisplayScreen : GameScreen
    {
        private SpriteFont Font { get { return LoadingScreen.font; } }

        public DebugDisplayScreen() : base("Debug Display")
        {
            Visible = false;
        }

        public override void Update()
        {
            if (Input.KeyDown(Keys.F1))
                Visible = !Visible;
        }

        public override void DrawUI(SpriteBatch spr)
        {
            int y = 140;

            for(int i = 0; i < Debug.DebugTexts.Count; i++)
            {
                string text = Debug.DebugTexts[i];
                var size = Font.MeasureString(text);

                int x = Screen.Width - ((int)size.X + 5);
                spr.DrawString(Font, text, new Vector2(x, y), Color.Black);
                y += (int)size.Y;
            }
        }
    }
}
