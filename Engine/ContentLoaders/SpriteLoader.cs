using Engine.Sprites;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.ContentLoaders
{
    public class SpriteLoader : ContentLoader
    {
        public SpriteLoader() : base(typeof(Sprite))
        {

        }

        public override object Load(ContentManager content, string path)
        {
            return JContent.Packer.TryPack(content.Load<Texture2D>(path));
        }

        public override void Unload(object obj)
        {
            
        }
    }
}
