using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.ContentLoaders
{
    public class FontLoader : ContentLoader
    {
        public FontLoader() : base(typeof(SpriteFont))
        {

        }

        public override object Load(ContentManager content, string path)
        {
            return content.Load<SpriteFont>(path);
        }

        public override void Unload(object obj)
        {
            (obj as SpriteFont).Texture?.Dispose();
        }
    }
}
