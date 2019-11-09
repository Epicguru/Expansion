using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Tiles
{
    public class ChunkGraphics : IDisposable
    {
        public static int TotalTextureCount { get; private set; } = 0;

        public bool CanRender { get { return Texture != null && !Texture.IsDisposed; } }
        public RenderTarget2D Texture { get; private set; }
        public Chunk Chunk { get; internal set; }

        public ChunkGraphics(Chunk c)
        {
            this.Chunk = c;            
        }

        public void Create()
        {
            if(Texture != null)
            {
                Debug.Warn("Texture is already created for this chunk, why call twice?");
                return;
            }

            Texture = new RenderTarget2D(JEngine.MainGraphicsDevice, Chunk.SIZE * Tile.SIZE, Chunk.SIZE * Tile.SIZE);
            Texture.Name = "Chunk Render Target";

            TotalTextureCount++;
        }

        public void Dispose()
        {
            if(Texture != null)
            {
                Texture.Dispose();
                Texture = null;
                TotalTextureCount--;
            }            
        }
    }
}
