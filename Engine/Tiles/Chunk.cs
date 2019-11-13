using Engine.Entities;
using Engine.MathUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Tiles
{
    public class Chunk : IDisposable
    {
        public static int TotalCount { get; private set; }
        /// <summary>
        /// The size, in tiles, of each chunk in both the horizontal and vertical directions.
        /// </summary>
        public const int SIZE = 32;

        public int X { get; private set; }
        public int Y { get; private set; }
        public long ID { get; private set; }
        public bool RequiresRedraw { get; private set; } = false;
        public float TimeSinceRendered { get; private set; }
        public float TimeSinceNeeded { get; private set; }
        public int FramesSinceRendered { get; private set; }
        public int FramesSinceNeeded { get; private set; }
        public int EntityCount { get { return entities?.Count ?? 0; } }
        public ChunkGraphics Graphics { get; private set; }

        internal List<Entity> entities = new List<Entity>();
        private readonly Tile[] tiles;

        public Chunk(int x, int y)
        {
            tiles = new Tile[SIZE * SIZE];
            this.X = x;
            this.Y = y;
            this.ID = ((long)x << 32) | (uint)y;
            
            TotalCount++;

            Graphics = new ChunkGraphics(this);
        }

        ~Chunk()
        {
            TotalCount--;
        }

        public void ClearEntities()
        {
            entities.Clear();
        }

        public void RequestRedraw()
        {
            RequiresRedraw = true;
            if (!Graphics.CanRender)
                Graphics.Create();
        }

        public void UnloadGraphics()
        {
            Graphics.Dispose();
        }

        public void FlagAsNeeded()
        {
            TimeSinceNeeded = 0f;
            FramesSinceNeeded = 0;
        }

        public void FlagAsRendered()
        {
            TimeSinceRendered = 0f;
            FramesSinceRendered = 0;
        }

        public void Decay(float dt)
        {
            TimeSinceRendered += dt;
            FramesSinceRendered++;

            TimeSinceNeeded += dt;
            FramesSinceNeeded++;
        }

        public void SetTile(int localX, int localY, Tile tile)
        {
            if (localX < 0 || localY < 0)
                return;

            if (localX >= SIZE || localY >= SIZE)
                return;

            tiles[localX + localY * SIZE] = tile;

            RequiresRedraw = true;
        }

        public int GetIndex(int localX, int localY)
        {
            return localX + localY * SIZE;
        }

        public Point GetLocalCoords(int localIndex)
        {
            return new Point(localIndex % SIZE, localIndex / SIZE);
        }

        public Point LocalToWorldCoords(Point localCoords)
        {
            return new Point(localCoords.X + X * SIZE, localCoords.Y + Y * SIZE);
        }

        public Point GetWorldCoords(int localIndex)
        {
            return LocalToWorldCoords(GetLocalCoords(localIndex));
        }

        public void Redraw(SpriteBatch spr)
        {
            JEngine.MainGraphicsDevice.Clear(Color.TransparentBlack);

            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    var tile = tiles[x + y * SIZE];
                    if (tile.ID != 0)
                    {
                        tile.Draw(spr, this, x, y);
                    }
                }
            }

            RequiresRedraw = false;
        }

        private static Noise noise = new Noise(69420);
        public void LoadData()
        {
            const float SCALE = 0.008f;
            float baseX = X * SCALE * SIZE;
            float baseY = Y * SCALE * SIZE;

            float min = float.MaxValue;
            float max = float.MinValue;
            noise.SetPerlinPersistence(0.4f);

            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    float n = noise.GetPerlin(baseX + x * SCALE, baseY + y * SCALE, 0);

                    // Remap noise form -1 -> 1 to 0 -> 1
                    n *= 0.5f;
                    n += 0.5f;

                    Color c = Color.Blue;
                    if (n > 0.9f)
                        c = Color.NavajoWhite;
                    else if (n > 0.8f)
                        c = Color.DarkSlateGray;
                    else if (n > 0.65f)
                        c = Color.DimGray;
                    else if (n > 0.5f)
                        c = Color.LawnGreen;
                    else if (n > 0.25f)
                        c = Color.SandyBrown;
                    else if (n > 0.2f)
                        c = Color.Yellow;

                    if (n > max)
                        max = n;
                    if (n < min)
                        min = n;

                    tiles[x + y * SIZE] = new Tile(1, ColorCache.EnsureColor(c));
                }
            }
        }

        public void Dispose()
        {
            if(Graphics != null)
            {
                Graphics.Dispose();
                Graphics.Chunk = null;
                Graphics = null;
            }
           
            RequiresRedraw = false;
            foreach (var e in entities)
            {
                e.CurrentChunk = null;
            }
            entities?.Clear();
            entities = null;
        }
    }
}
