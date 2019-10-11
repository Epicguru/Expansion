using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Loaders
{
    public class FontLoader : ContentLoader
    {
        public FontLoader() : base(typeof(SpriteFont))
        {

        }

        public override object Load(string path, out string error, params object[] args)
        {
            error = null;

            if(args == null || args.Length == 0)
            {
                // Load internally.
                return Engine.XNAContent.Load<SpriteFont>(path);
            }
            else
            {
                // Load externally.
                throw new NotImplementedException();
            }
        }
    }
}
