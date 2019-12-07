
using Engine.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Engine.Entities
{
    public class EntityManager : IDisposable
    {
        public int EntityCount { get; private set; }
        public int MaxEntityCount { get { return entities.Length - 1; } }
        public bool DrawPathfindingDebug { get; set; } = false;

        private List<Entity> allEntities = new List<Entity>();
        private Queue<Entity> toAdd = new Queue<Entity>();
        private Entity[] entities = new Entity[ushort.MaxValue + 1];
        private int highestID = 1;

        internal void StartRegister(Entity e)
        {
            if (e == null)
            {
                Debug.Error("Entity is null, cannot register.");
                return;
            }

            if (e.ID != 0)
            {
                Debug.Error($"Entity {e} is already registered somewhere! Expected ID 0, has ID {e.ID}.");
                return;
            }

            if (e.RemovePending)
            {
                Debug.Warn($"Trying to register an entity that is already flagged for removal... Will not be registered.");
                return;
            }

            toAdd.Enqueue(e);
        }

        internal void InstantRegister(Entity e)
        {
            // Is this such a good idea? Note to self, this is used for tile entities
            // that need an ID immediately after spawned.
            // Might break things, might not. Who knows.

            Register(e);
        }

        private ushort Register(Entity e)
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
            e.InternalOnRegistered();

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

            e.InternalOnDestroyed();
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

        public Entity Get(ushort ID)
        {
            if (ID == 0)
                return null;

            return entities[ID];
        }

        public void UpdateAll()
        {
            // Add in new entities...
            while(toAdd.Count > 0)
            {
                var pending = toAdd.Dequeue();
                Register(pending);
            }

            for (int i = 0; i < allEntities.Count; i++)
            {
                var e = allEntities[i];
                if (e.RemovePending)                
                    continue;       

                e.InternalUpdate();
            }

            foreach (var chunk in JEngine.TileMap.GetLoadedChunks())
            {
                chunk.ClearEntities();
            }
            foreach (var e in allEntities)
            {
                e.CurrentChunk = null;
                if (e.DoChunkParenting)
                {
                    var coords = e.GetChunkCoords();

                    // Update chunk...
                    var chunk = JEngine.TileMap.GetChunk(coords.X, coords.Y);
                    if (chunk == null)
                        chunk = JEngine.TileMap.LoadChunk(coords.X, coords.Y, true);

                    chunk.entities.Add(e);
                    e.CurrentChunk = chunk;
                }
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
                    i--;
                    Unregister(e);
                    continue;                                
                }               

                if(e.CullingMode == EntityCullingMode.Bounds || (!e.DoChunkParenting && e.CullingMode != EntityCullingMode.None))
                {
                    if (!cam.ContainsPoint(e.Position, e.Size.X * 2f, e.Size.Y * 2f))
                        continue;
                }
                else if(e.CullingMode == EntityCullingMode.Chunk)
                {
                    if (e.CurrentChunk == null || !e.CurrentChunk.Graphics.CanRender)
                        continue;
                }

                e.Draw(spr);

                if (DrawPathfindingDebug)
                {
                    if(e is ActiveEntity)
                    {
                        var e2 = (ActiveEntity)e;
                        e2.DrawDebugPath(spr);
                    }
                }
            }
        }

        public IEnumerable<Entity> GetAllInRange(Vector2 point, float radius, bool fromBoundsCenter = true)
        {
            if(radius <= 0)
            {
                Debug.Error("Radius must be greater than zero in GetAllInRange.");
                yield return null;
            }

            float square = radius * radius;
            var layer = JEngine.TileMap;

            var topLeft = layer.TileToChunkCoords(layer.PixelToTileCoords((int)(point.X - radius), (int)(point.Y - radius)));
            var bottomRight = layer.TileToChunkCoords(layer.PixelToTileCoords((int)(point.X + radius), (int)(point.Y + radius)));

            for (int x = topLeft.X; x <= bottomRight.X; x++)
            {
                for (int y = topLeft.Y; y <= bottomRight.Y; y++)
                {
                    var chunk = layer.GetChunk(x, y);
                    if (chunk == null)
                        continue;
                    
                    var entities = chunk.entities;
                    foreach (var entity in entities)
                    {
                        Vector2 pos = Vector2.Zero;
                        if (fromBoundsCenter)
                        {
                            // Position is considered to be the center of the bounds. This tends to match sprite graphics.
                            pos = entity.Position + entity.Size * 0.5f;
                        }
                        else
                        {
                            // Position is considered as the raw drawing position of the entity.
                            pos = entity.Position;
                        }

                        Vector2 diff = point - pos;
                        float squareDiff = diff.LengthSquared();

                        if(squareDiff <= square)
                        {
                            yield return entity;
                        }
                    }
                }
            }
        }

        public void SerializeAll(IOWriter writer)
        {
            // First of all, compress names to ushorts using a map.
            Dictionary<Type, (string name, ushort id)> map = new Dictionary<Type, (string, ushort)>();
            ushort id = 0;
            foreach (var e in allEntities)
            {
                Type t = e.GetType();
                if (!map.ContainsKey(t))
                {
                    map.Add(t, (t.AssemblyQualifiedName, id));
                    id++;
                }
            }

            // Write map...
            writer.Write(map.Count);
            foreach (var pair in map)
            {
                writer.Write(pair.Value.name);
            }

            writer.Write(allEntities.Count);
            foreach (var e in allEntities)
            {
                writer.Write(map[e.GetType()].id);
                writer.Write(e);
            }
        }

        public void DeserializeAllNew(IOReader reader)
        {
            ClearEntities();

            // Read name map...
            Type[] blankParams = new Type[0];
            object[] blankArgs = new object[0];
            int count = reader.ReadInt32();
            Type[] types = new Type[count];
            ConstructorInfo[] constructors = new ConstructorInfo[count];
            for (int i = 0; i < count; i++)
            {
                string typeName = reader.ReadString();

                Type t = Type.GetType(typeName, false, false);
                if (t == null)
                    throw new Exception($"Entity of type {typeName} could not be found, so cannot be loaded from disk! Reading cannot continue because data length is unknown.");

                var constructor = t.GetConstructor(blankParams);
                if (constructor == null)
                    throw new Exception($"Entity of type {typeName} does not have a no-parameter constructor, so cannot be loaded. Reading cannot continue since data length is unknown.");
                types[i] = t;
                constructors[i] = constructor;
            }

            

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                ushort typeMapIndex = reader.ReadUInt16();
                var constructor = constructors[typeMapIndex];

                var obj = constructor.Invoke(blankArgs);

                // No need to register: Entities should auto register, especially since the default constructor is being used.
                Entity e = obj as Entity;
                e.Deserialize(reader);
            }
        }

        public void ClearEntities()
        {
            while (EntityCount != 0)
            {
                Unregister(allEntities[0]);
            }
        }

        public void Dispose()
        {
            ClearEntities();

            entities = null;
            allEntities?.Clear();
            allEntities = null;
            toAdd?.Clear();
            toAdd = null;
            highestID = 1;
        }
    }
}
