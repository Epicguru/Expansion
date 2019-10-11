using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Screens
{
    internal class LoadingScreen : GameScreen
    {
        private SpriteFont font;

        public LoadingScreen() : base("Engine loading screen")
        {
        }

        public override void LoadContent(Content contentManager)
        {
            font = contentManager.Load<SpriteFont>("MediumFont");   
        }

        public override void DrawUI(SpriteBatch spr)
        {
            spr.DrawString(font, "Hello, world!", Vector2.Zero, Color.Black);
        }
    }
}
