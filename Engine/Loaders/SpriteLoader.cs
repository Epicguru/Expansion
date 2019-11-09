#define ALLOW_LATE_LOAD

using Engine.Sprites;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Loaders
{
    public class SpriteLoader : ContentLoader
    {
        public SpriteLoader() : base(typeof(Sprite))
        {
        }

        public override object Load(string path, out string error, params object[] args)
        {
            // Currently only supports internal mode.
            if(Content.SpritePacker == null)
            {
#if ALLOW_LATE_LOAD
                Debug.Error($"Sprite {path} is loading when no sprite packer is active: this sprite will be unoptimized. Please load sprites in LoadContent!");
                Texture2D finalTex = Content.Load<Texture2D>(path);
                Sprite spr = new Sprite(finalTex);
                error = null;
                return spr;
#else
                error = "Trying to load sprite, but no sprite packer is available.";
                return null;
#endif
            }

            Texture2D texture = Content.Load<Texture2D>(path);
            error = null;
            return Content.SpritePacker.TryPack(texture);
        }
    }
}
