using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Engine.Loaders
{
    public class TextureLoader : ContentLoader
    {
        public TextureLoader() : base(typeof(Texture2D))
        {

        }

        public override object Load(string path, out string error, params object[] args)
        {
            if(args == null || args.Length == 0)
            {
                // Internal mode:
                error = null;
                try
                {
                    return Engine.XNAContent.Load<Texture2D>(path);
                }
                catch(Exception e)
                {
                    Debug.Error("Failed to load texture. (internal)", e);
                    error = "Failed to load texture. (internal)" + e.Message;
                    return null;
                }
            }
            else
            {
                // External mode:
                error = null;
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open))
                    {
                        return Texture2D.FromStream(Engine.MainGraphicsDevice, fs);
                    }
                }
                catch (Exception e)
                {
                    Debug.Error("Failed to load texture. (external)", e);
                    error = "Failed to load texture. (external) " + e.Message;
                    return null;
                }
            }                       
        }
    }
}
