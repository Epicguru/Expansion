
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Entities
{
    public class EntityManager : IDisposable
    {
        public int EntityCount { get; private set; }
        public int MaxEntityCount { get { return entities.Length - 1; } }

        private List<Entity> allEntities = new List<Entity>();
        private Entity[] entities = new Entity[ushort.MaxValue + 1];
        private int highestID = 1;

        internal ushort Register(Entity e)
        {
            if(e == null)
            {
                Debug.Error("Entity is null, cannot register.");
                return 0;
            }

            if(e.ID != 0)
            {
                Debug.Error($"Entity {e} is already registered somewhere! Expected ID 0, has ID {e.ID}.");
                return 0;
            }

            if (e.RemovePending)
            {
                Debug.Warn($"Trying to register an entity that is already flagged for removal... Will not be registered.");
                return 0;
            }

            int newIndex = GetNextID(highestID);
            if(newIndex == -1)
            {
                Debug.Error("Cannot register new Entity, max entity count exceeded!");
                return 0;
            }

            highestID = newIndex + 1;
            if (highestID >= entities.Length)
                highestID = 1;
            e.ID = (ushort)newIndex;
            EntityCount++;
            allEntities.Add(e);
            entities[newIndex] = e;

            return e.ID;
        }

        private void Unregister(Entity e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if(e.ID == 0)
            {
                Debug.Error($"Entity {e.Name} has no ID, so cannot be unregistred.");
                return;
            }

            entities[e.ID] = null;
            EntityCount--;
            allEntities.Remove(e);
            e.ID = 0;

            var chunk = e.CurrentChunk;
            if (chunk != null)
                chunk.entities.Remove(e);
            e.CurrentChunk = null;
        }

        /// <summary>
        /// Gets the next vacant ID starting from the provided ID (inclusive) and wrapping around the end of the id list.
        /// </summary>
        /// <param name="startID">The starting ID.</param>
        /// <returns></returns>
        private int GetNextID(int startID)
        {
            if (startID < 1 || startID >= entities.Length)
                throw new ArgumentOutOfRangeException(nameof(startID), $"Start ID out of bounds. Must be between 1 and {entities.Length - 1} inclusive.");

            int id = startID;
            for (int c = 0; c < entities.Length + 1; c++)
            {
                if(entities[id] == null)
                {
                    return id;
                }
                else
                {
                    id++;
                    if (id >= entities.Length)
                        id = 1;
                }
            }

            return -1;
        }

        public void UpdateAll()
        {
            for (int i = 0; i < allEntities.Count; i++)
            {
                var e = allEntities[i];
                if (e.RemovePending)
                {
                    Unregister(e);
                    e.InternalOnDestroyed();
                    i--;
                    continue;
                }

                var coords = e.GetChunkCoords();
                if (e.CurrentChunk == null || (e.CurrentChunk.X != coords.X || e.CurrentChunk.Y != coords.Y))
                {
                    // Update chunk...
                    var chunk = JEngine.TileMap.GetChunk(coords.X, coords.Y);
                    if (chunk == null)
                        chunk = JEngine.TileMap.LoadChunk(coords.X, coords.Y);

                    e.CurrentChunk?.entities.Remove(e);
                    chunk.entities.Add(e);
                    e.CurrentChunk = chunk;
                }

                e.Update();
            }
        }

        public void DrawAll(SpriteBatch spr)
        {
            var cam = JEngine.Camera;
            for (int i = 0; i < allEntities.Count; i++)
            {
                var e = allEntities[i];
                if (e.RemovePending)
                {
                    Unregister(e);
                    e.InternalOnDestroyed();
                    i--;
                    continue;
                }                

                if(e.CullingMode == EntityCullingMode.Bounds)
                {
                    if (!cam.ContainsPoint(e.Position, e.Size.X * 2f, e.Size.Y * 2f))
                        continue;
                }

                e.Draw(spr);
            }
        }

        public void Dispose()
        {
            while(EntityCount != 0)
            {
                Unregister(allEntities[0]);
            }

            entities = null;
            allEntities.Clear();
            highestID = 1;
        }
    }
}
