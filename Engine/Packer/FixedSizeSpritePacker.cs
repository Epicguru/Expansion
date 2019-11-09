
using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Packer
{
    public class FixedSizeSpritePacker
    {
        // URGTODO pack in order of width or height.
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Padding { get; private set; }

        private PackerRegion RootRegion;
        private List<PrePackedTexture> prePacked = new List<PrePackedTexture>();
        private int unammedIndex = 0;
        private Texture2D texture;

        public FixedSizeSpritePacker(int width, int height, int padding)
        {
            if (width <= 1)
                    throw new SpriteException($"Invalid packer width: {width}");
            if (height <= 1)
                throw new SpriteException($"Invalid packer height: {width}");
            if (padding < 0)
                padding = 0;

            Padding = padding;
            Width = width;
            Height = height;
            RootRegion = new PackerRegion(new Rectangle(0, 0, width, height));

            texture = SpriteAtlas.CreateTexture(width, height);
        }

        public PackerRegion FindRegion(PackerRegion region, int w, int h)
        {
            if (region.Used)
            {
                var right = this.FindRegion(region.Right, w, h);
                if(right != null)                
                    return right;
                var down = this.FindRegion(region.Down, w, h);
                return down;
            }
            else if(w < region.Bounds.Width && h < region.Bounds.Height)
            {
                return region;
            }
            else
            {
                return null;
            }
        }

        private Point SplitRegion(PackerRegion region, int w, int h)
        {
            Debug.Assert(region != null, "Region is null, cannot split it!");
            Debug.Assert(!region.Used, "Region is already used!");

            region.Used = true;
            region.Down = new PackerRegion(new Rectangle(region.Bounds.X, region.Bounds.Y + h, region.Bounds.Width, region.Bounds.Height - h));
            region.Right = new PackerRegion(new Rectangle(region.Bounds.X + w, region.Bounds.Y, region.Bounds.Width - w, h));

            return region.Bounds.Location;
        }

        private bool TryFit(int width, int height, out Point packedPosition)
        {
            var node = FindRegion(RootRegion, width, height);
            if(node == null)
            {
                packedPosition = Point.Zero;
                return false;
            }
            else
            {
                packedPosition = SplitRegion(node, width, height);
                return true;
            }
        }

        public Sprite TryPack(Texture2D texture, string customName = null)
        {
            if (texture == null)
            {
                return null;
            }

            bool canFit = TryFit(texture.Width + Padding * 2, texture.Height + Padding * 2, out Point pos);
            if (canFit)
            {
                PrePackedTexture ppt = new PrePackedTexture(texture, pos + new Point(Padding, Padding), customName ?? (texture.Name ?? $"NoName{unammedIndex++}"), this.texture);
                prePacked.Add(ppt);

                return ppt.FutureSprite;
            }
            else
            {
                return null;
            }
        }

        public void TryPackAll(IEnumerable<Texture2D> textures)
        {
            foreach (var tex in textures)
            {
                TryPack(tex);
            }
        }

        public void Clear()
        {
            prePacked.Clear();
            RootRegion.Dispose();
            RootRegion = new PackerRegion(new Rectangle(0, 0, Width, Height));
            unammedIndex = 0;
        }

        // TODO create a 'shrink' option that discards unused margins to minimize texture size.
        public SpriteAtlas CreateSpriteAtlas(bool disposeTextures)
        {
            SpriteAtlas a = new SpriteAtlas(Width, Height);
            a.SetTexture(texture);
            foreach (var item in prePacked)
            {
                a.Blit(item.Texture, item.PackedPosition, Padding);
                a.AddSprite(item.FutureSprite);
            }

            if (disposeTextures)
            {
                foreach (var item in prePacked)
                {
                    item.Texture?.Dispose();
                    item.Texture = null;
                }
            }

            a.Finish();

            return a;
        }

        public void Dispose()
        {
            RootRegion?.Dispose();
            RootRegion = null;
            prePacked.Clear();
            prePacked = null;
            unammedIndex = 0;
            Width = 0;
            Height = 0;
        }

        public class PrePackedTexture
        {
            public Texture2D Texture;
            public Point PackedPosition;
            public string Name;
            public Sprite FutureSprite;

            public PrePackedTexture(Texture2D tex, Point pos, string name, Texture2D finalTexture)
            {
                this.Texture = tex;
                this.PackedPosition = pos;
                this.Name = name;
                FutureSprite = new Sprite(name, null, new Rectangle(pos, new Point(tex.Width, tex.Height)));
                FutureSprite.Texture = finalTexture;
            }
        }

        public class PackerRegion : IDisposable
        {
            public Rectangle Bounds;
            public bool Used = false;
            public PackerRegion Down, Right;

            public PackerRegion(Rectangle bounds)
            {
                this.Bounds = bounds;
            }

            public void Dispose()
            {
                Down?.Dispose();
                Down = null;
                Right?.Dispose();
                Right = null;
            }
        }
    }
}