using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Screens
{
    internal class TestScreen : GameScreen
    {
        public TestScreen() : base("Engine testing screen") { }

        private Texture2D texture;

        public override void Init()
        {
            Debug.Trace("Init");
        }

        public override void LoadContent(Content contentManager)
        {
            Debug.Trace("Load content");
            texture = contentManager.Load<Texture2D>(@"C:\Users\James.000\Pictures\Glory.png", "external");
        }

        private bool canPost = true;
        public override void Update()
        {
            if (canPost)
            {
                canPost = false;
                Engine.AddAction(() =>
                {
                    Engine.GameWindow.Title = $"Engine: {Loop.Framerate:F0}fps";
                    canPost = true;
                });
            }
        }

        public override void Draw(SpriteBatch spr)
        {
            spr.Draw(texture, Vector2.Zero, Color.White);
        }
    }
}
