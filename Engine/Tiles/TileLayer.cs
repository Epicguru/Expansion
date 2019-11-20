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

        public Tile GetTile(int x, int y, int z, bool load = false, bool redrawIfLoad = false)
        {
            var cp = TileToChunkCoords(x, y);
            Chunk c = GetChunk(cp);

            if (c == null)
            {
                if (load)
                    c = LoadChunk(cp.X, cp.Y, redrawIfLoad);
                else
                    return new Tile(0, 0);
            }

            return c.GetTileFast(x - c.X * Chunk.SIZE, y - c.Y * Chunk.SIZE, z);
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

        public Tile SetTile(int x, int y, int z, Tile tile, bool redrawSurroundings = true)
        {
            Point chunkCoords = TileToChunkCoords(x, y);
            long id = MakeChunkID(chunkCoords.X, chunkCoords.Y);

            Chunk c = null;
            if (!IsChunkLoaded(id))
            {
                if (AutoLoadChunks)
                {
                    c = LoadChunk(id, redrawSurroundings);
                    //Debug.Trace($"Auto-loaded chunk {chunkCoords.X}, {chunkCoords.Y}");
                }
                else
                {
                    Debug.Error($"Tried to set tile {tile} at world coords ({x}, {y}) but the corresponding chunk ({chunkCoords.X}, {chunkCoords.Y} [{id}]) is not loaded." +
                        $" Check if chunks are loaded or set TileLayer.AutoLoadChunks to true.");
                    return Tile.Blank;
                }
            }
            else
            {
                c = loadedChunks[id];
            }

            int localX = x - c.X * Chunk.SIZE;
            int localY = y - c.Y * Chunk.SIZE;

            Tile current = c.GetTileFast(localX, localY, z);
            if (!current.IsBlank)
            {
                current.Def.UponRemoved(ref current, c, localY, localY, z, false);
            }

            if (!tile.IsBlank)
            {
                int r = tile.Def.RedrawRadius;
                if(r != 0)
                {
                    bool updateLeft = localX < r;
                    bool updateTop = localY < r;
                    bool updateRight = localX >= Chunk.SIZE - r;
                    bool updateBottom = localY >= Chunk.SIZE - r;

                    if (updateLeft)
                        GetChunk(c.X - 1, c.Y)?.RequestRedraw();
                    if (updateRight)
                        GetChunk(c.X + 1, c.Y)?.RequestRedraw();
                    if (updateBottom)
                        GetChunk(c.X, c.Y + 1)?.RequestRedraw();
                    if(updateTop)
                        GetChunk(c.X, c.Y - 1)?.RequestRedraw();
                }

                // Notify tile def of placement.
                tile.Def.UponPlaced(ref tile, c, localX, localY, z, false);
            }           

            c.SetTile(localX, localY, z, tile);
            return tile;
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

        public Chunk LoadChunk(long chunkID, bool redrawSurroundings)
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

            if (redrawSurroundings)
            {
                // Redraw chunks around it.
                GetChunk(coords.X - 1, coords.Y)?.RequestRedraw();
                GetChunk(coords.X + 1, coords.Y)?.RequestRedraw();

                GetChunk(coords.X, coords.Y + 1)?.RequestRedraw();
                GetChunk(coords.X, coords.Y - 1)?.RequestRedraw();

                GetChunk(coords.X + 1, coords.Y - 1)?.RequestRedraw();
                GetChunk(coords.X - 1, coords.Y - 1)?.RequestRedraw();
                GetChunk(coords.X + 1, coords.Y + 1)?.RequestRedraw();
                GetChunk(coords.X - 1, coords.Y + 1)?.RequestRedraw();
            }

            chunk.NotifyAllPlaced();

            return chunk;
        }

        public Chunk LoadChunk(int x, int y, bool redrawSurroudings)
        {
            return LoadChunk(MakeChunkID(x, y), redrawSurroudings);
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
            // TODO pool textures.
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
 