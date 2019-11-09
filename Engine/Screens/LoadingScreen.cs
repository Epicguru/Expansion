using Engine.GUI;
using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Screens
{
    internal class LoadingScreen : GameScreen
    {
        internal static SpriteFont font;
        internal static NinePatch np, buttonNP;

        public LoadingScreen() : base("Engine loading screen")
        {
            Visible = false;
        }

        public override void LoadContent(Content contentManager)
        {
            font = contentManager.Load<SpriteFont>("MediumFont");

            np = new NinePatch(contentManager.Load<Sprite>("TestNP"), new Rectangle(5, 5, 6, 6));
            buttonNP = new NinePatch(contentManager.Load<Sprite>("ButtonNP"), new Rectangle(10, 10, 12, 12));
        }

        public override void DrawUI(SpriteBatch spr)
        {
            spr.DrawString(font, "Hello, world!", Vector2.Zero, Color.Black);

            np.Draw(spr, new Rectangle(100, 100, 16, 16), Color.White);
            np.Draw(spr, new Rectangle(120, 120, 40, 40), Color.White);
            np.Draw(spr, new Rectangle(170, 120, 5, 5), Color.White);
            buttonNP.Draw(spr, new Rectangle(300, 300, Input.MousePos.X - 300, Input.MousePos.Y - 300), Color.DarkOliveGreen);

            UILayout.Button("Button 1", Color.LawnGreen);
            UILayout.BeginHorizontal();
            UILayout.Button("Button 2 (below)", Color.DarkOrchid);
            UILayout.BeginVertical();
            UILayout.Button("Button 3 (right side)", Color.MediumAquamarine);
            UILayout.Button("Another button (but below B3)", Color.Yellow);
            UILayout.EndVertical();
            UILayout.EndHorizontal();
            if(UILayout.Button("Button below all!", Color.White))
            {
                Debug.Log("Pressed!");
            }
        }
    }
}
