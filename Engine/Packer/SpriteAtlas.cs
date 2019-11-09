using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Packer
{
    public class SpriteAtlas : IDisposable
    {
        public Texture2D Texture { get; private set; }
        public int Width { get { return Texture?.Width ?? 0; } }
        public int Height { get { return Texture?.Height ?? 0; } }
        public Sprite[] Sprites { get; private set; }
        public bool IsLoaded { get; private set; }

        private Dictionary<string, Sprite> namedSprites = new Dictionary<string, Sprite>();
        private List<Sprite> tempSprites = new List<Sprite>();

        internal SpriteAtlas(int w, int h)
        {
            IsLoaded = false;
        }

        internal static Texture2D CreateTexture(int w, int h)
        {
            Texture2D texture = new Texture2D(JEngine.MainGraphicsDevice, w, h, false, SurfaceFormat.Color);
            return texture;
        }

        internal void SetTexture(Texture2D texture)
        {
            this.Texture = texture;
        }

        internal void Blit(Texture2D source, Point position, int padding)
        {
            if (source == null)
                return;

            Color[] data = new Color[source.Width * source.Height];
            source.GetData(data);
            Texture.SetData(0, new Rectangle(position, source.Bounds.Size), data, 0, data.Length);

            if(padding != 0)
            {
                // Left side.
                Color[] side = new Color[source.Height * padding];
                for (int x = 0; x < padding; x++)
                {
                    for (int y = 0; y < source.Height; y++)
                    {
                        side[x + y * padding] = data[y * source.Width];
                    }
                }
                Texture.SetData(0, new Rectangle(position.X - padding, position.Y, padding, source.Height), side, 0, side.Length);

                // Right side.
                for (int x = 0; x < padding; x++)
                {
                    for (int y = 0; y < source.Height; y++)
                    {
                        side[x + y * padding] = data[(source.Width - 1) + y * source.Width];
                    }
                }
                Texture.SetData(0, new Rectangle(position.X + source.Width, position.Y, padding, source.Height), side, 0, side.Length);

                // Top side.
                Color[] tops = new Color[source.Width * padding];
                for (int x = 0; x < source.Width; x++)
                {
                    for (int y = 0; y < padding; y++)
                    {
                        tops[x + y * source.Width] = data[x];
                    }
                }
                Texture.SetData(0, new Rectangle(position.X, position.Y - padding, source.Width, padding), tops, 0, tops.Length);

                // Bottom side.
                for (int x = 0; x < source.Width; x++)
                {
                    for (int y = 0; y < padding; y++)
                    {
                        tops[x + y * source.Width] = data[x + (source.Height - 1) * source.Width];
                    }
                }
                Texture.SetData(0, new Rectangle(position.X, position.Y + source.Height, source.Width, padding), tops, 0, tops.Length);
            }
        }

        internal void AddSprite(Sprite spr)
        {
            if(spr != null)
                this.tempSprites.Add(spr);
        }

        internal void Finish()
        {
            Debug.Assert(!IsLoaded);

            IsLoaded = true;
            Sprites = tempSprites.ToArray();
            foreach (var spr in Sprites)
            {
                spr.Texture = Texture;
            }
        }

        public override string ToString()
        {
            return $"SpriteAtlas with {Sprites?.Length ?? 0} sprites. {Width}x{Height} pixels.";
        }

        public void Dispose()
        {
            Texture?.Dispose();
            Texture = null;
            Sprites = null;
            namedSprites?.Clear();
            namedSprites = null;
            tempSprites?.Clear();
            tempSprites = null;
        }
    }
}
