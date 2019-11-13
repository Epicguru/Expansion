using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Engine.Tiles
{
    public class TileLayer : IDisposable
    {
        public static bool AutoLoadChunks = true;

        public int LoadedChunkCount { get { return loadedChunks.Count; } }

        private Dictionary<long, Chunk> loadedChunks = new Dictionary<long, Chunk>();

        public IEnumerable<Chunk> GetRedrawChunks()
        {
            foreach (var chunk in loadedChunks.Values)
            {
                if (chunk.RequiresRedraw)
                    yield return chunk;
            }
        }

        public IEnumerable<Chunk> GetLoadedChunks()
        {
            return loadedChunks.Values;
        }

        public bool IsChunkLoaded(long chunkID)
        {
            return loadedChunks.ContainsKey(chunkID);
        }

        public bool IsChunkLoaded(int chunkX, int chunkY)
        {
            return IsChunkLoaded(MakeChunkID(chunkX, chunkY));
        }

        public Chunk GetChunk(long id)
        {
            if (loadedChunks.ContainsKey(id))
                return loadedChunks[id];
            else
                return null;
        }

        public Chunk GetChunk(int x, int y)
        {
            long id = MakeChunkID(x, y);
            return GetChunk(id);
        }

        public Chunk GetChunk(Point pos)
        {
            return GetChunk(pos.X, pos.Y);
        }

        private Chunk MakeChunk(int cx, int cy)
        {
            return new Chunk(cx, cy);            
        }

        public void SetTile(int x, int y, Tile tile)
        {
            Point chunkCoords = TileToChunkCoords(x, y);
            long id = MakeChunkID(chunkCoords.X, chunkCoords.Y);

            Chunk c = null;
            if (!IsChunkLoaded(id))
            {
                if (AutoLoadChunks)
                {
                    c = LoadChunk(id);
                    //Debug.Trace($"Auto-loaded chunk {chunkCoords.X}, {chunkCoords.Y}");
                }
                else
                {
                    Debug.Error($"Tried to set tile {tile} at world coords ({x}, {y}) but the corresponding chunk ({chunkCoords.X}, {chunkCoords.Y} [{id}]) is not loaded." +
                        $" Check if chunks are loaded or set TileLayer.AutoLoadChunks to true.");
                    return;
                }
            }
            else
            {
                c = loadedChunks[id];
            }

            int localX = x - c.X * Chunk.SIZE;
            int localY = y - c.Y * Chunk.SIZE;

            c.SetTile(localX, localY, tile);
        }

        public Point TileToChunkCoords(Point tilePos)
        {
            return TileToChunkCoords(tilePos.X, tilePos.Y);
        }

        public Point TileToChunkCoords(int tileX, int tileY)
        {
            return new Point((int)Math.Floor((float)tileX / Chunk.SIZE), (int)Math.Floor((float)tileY / Chunk.SIZE));
        }

        public Point PixelToTileCoords(Point pixelCoords)
        {
            return PixelToTileCoords(pixelCoords.X, pixelCoords.Y);
        }

        public Point PixelToTileCoords(int px, int py)
        {
            return new Point((int)Math.Floor((float)px / Tile.SIZE), (int)Math.Floor((float)py / Tile.SIZE));
        }

        public Chunk LoadChunk(long chunkID)
        {
            if(IsChunkLoaded(chunkID))
            {
                Debug.Warn($"Chunk for ID {chunkID} is already loaded.");
                return null;
            }

            Point coords = MakeChunkCoords(chunkID);
            Chunk chunk = MakeChunk(coords.X, coords.Y);
            chunk.FlagAsNeeded();

            chunk.LoadData();
            //Debug.Trace($"Loaded {coords.X}, {coords.Y}");


            Debug.Assert(chunk.ID == chunkID, "Chunk ID not as expected or specified!");
            loadedChunks.Add(chunk.ID, chunk);

            return chunk;
        }

        public Chunk LoadChunk(int x, int y)
        {
            return LoadChunk(MakeChunkID(x, y));
        }

        public void UnloadChunk(long chunkID)
        {
            if (!IsChunkLoaded(chunkID))
            {
                Debug.Warn($"Chunk for ID {chunkID} is not loaded, cannot unload.");
                return;
            }

            var chunk = loadedChunks[chunkID];
            loadedChunks.Remove(chunkID);
            chunk.Dispose();
            // URGTODO pool textures.
            //var coords = MakeChunkCoords(chunkID);
            //Debug.Trace($"Unloaded {coords.X}, {coords.Y}");
        }

        public long MakeChunkID(int x, int y)
        {
            return ((long)x << 32) | (long)(uint)y;
        }

        public Point MakeChunkCoords(long id)
        {
            int x = (int)(id >> 32);
            int y = (int)(id & 0xffffffffL);
            return new Point(x, y);
        }

        public void Dispose()
        {

        }
    }
}
 