
using Engine.ContentLoaders;
using Engine.Packer;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    public class JContent : IDisposable
    {
        public int LoadedCount { get { return loaded.Count; } }
        public FixedSizeSpritePacker Packer { get; internal set; }

        private Dictionary<string, (Type type, object value)> loaded = new Dictionary<string, (Type, object)>();
        private Dictionary<Type, ContentLoader> loaders = new Dictionary<Type, ContentLoader>();

        public void RegisterLoader(ContentLoader loader)
        {
            if(loader == null)
            {
                Debug.Error("Null content loader was passed into register.");
                return;
            }

            if (loaders.ContainsKey(loader.TargetType))
            {
                Debug.Warn($"There is already a loader registered for type {loader.TargetType.FullName}. This new loader '{loader.GetType().FullName}' will not be registered.");
                return;
            }

            loader.JContent = this;
            loaders.Add(loader.TargetType, loader);
        }

        public void RemoveLoader<T>()
        {
            this.RemoveLoader(typeof(T));
        }

        public void RemoveLoader(Type type)
        {
            Debug.Assert(type != null, "Type cannot be null.");
            if (loaders.ContainsKey(type))
            {
                var loader = loaders[type];
                loader.JContent = null;
                loaders.Remove(type);
            }
        }

        public T Load<T>(string path) where T : class
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.Error($"Null or blank string passed into Load method for type {typeof(T).Name}.");
                return default(T);
            }
            path = path.Trim().ToLower();
            if (IsLoaded(path))
            {
                Debug.Warn($"Content at '{path}' is already loaded. Call Get() instead of Load().");
                return loaded[path] as T;
            }
            if (!HasLoaderFor<T>())
            {
                Debug.Error($"No loader exists for type {typeof(T).Name}.");
                return default(T);
            }

            T created = loaders[typeof(T)].Load(JEngine.XNAContent, path) as T;
            loaded.Add(path, (typeof(T), created));

            return created;
        }

        public void Unload(string path)
        {            
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.Error($"Null or blank string passed into Unload method.");
                return;
            }
            path = path.Trim().ToLower();

            var pair = loaded[path];
            var loader = loaders[pair.type];

            loader.Unload(pair.value);

            loaded.Remove(path);
        }

        public void UnloadAll()
        {
            string[] toUnload = loaded.Keys.ToArray();
            foreach (var path in toUnload)
            {
                Unload(path);
            }
        }

        public T Get<T>(string path) where T : class
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.Error($"Null or blank string passed into Get method for type {typeof(T).Name}.");
                return default(T);
            }

            return loaded[path.Trim().ToLower()].value as T;
        }

        public bool HasLoaderFor<T>()
        {
            return loaders.ContainsKey(typeof(T));
        }

        public bool IsLoaded(string path)
        {
            return loaded.ContainsKey(path);
        }

        public void Dispose()
        {
            if(loaded != null)
            {
                UnloadAll();
                loaded.Clear();
                loaded = null;
            }

            if(loaders != null)
            {
                foreach (var pair in loaders)
                {
                    pair.Value.Dispose();
                }
                loaders.Clear();
                loaders = null;
            }
        }
    }
}
