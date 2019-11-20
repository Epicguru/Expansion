using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.ContentLoaders
{
    public class TextureLoader : ContentLoader
    {
        public TextureLoader() : base(typeof(Texture2D))
        {

        }

        public override object Load(ContentManager manager, string path)
        {
            return manager.Load<Texture2D>(path);
        }

        public override void Unload(object obj)
        {
            (obj as Texture2D).Dispose();
        }
    }
}
