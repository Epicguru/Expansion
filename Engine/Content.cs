
using Engine.Packer;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine
{
    /// <summary>
    /// The Content class is used to load textures, sounds and more.
    /// </summary>
    public class Content : IDisposable
    {
        public FixedSizeSpritePacker SpritePacker;

        private readonly Dictionary<Type, ContentLoader> loaders = new Dictionary<Type, ContentLoader>();
        private Stopwatch watch = new Stopwatch();

        /// <summary>
        /// Adds a content loader to this Content. This gives the ability to load new types of content.
        /// </summary>
        /// <param name="loader"></param>
        public void RegisterLoader(ContentLoader loader)
        {
            if (loader == null)
            {
                Debug.Warn("Registering loader is null.");
                return;
            }

            if (HasLoaderFor(loader.TargetType))
            {
                Debug.Warn($"Already have a loader for type {loader.TargetType.FullName}.");
                return;
            }

            loaders.Add(loader.TargetType, loader);
            loader.Content = this;
        }

        /// <summary>
        /// Synchronously loads content given a path.
        /// </summary>
        public T Load<T>(string path, params object[] args) where T : class
        {
            // TODO catch exception from loaders.
            if (HasLoaderFor<T>())
            {
                var loader = loaders[typeof(T)];
                watch.Reset();
                watch.Start();
                object obj = loader.Load(path, out string errorMsg, args);
                watch.Stop();

                Debug.Trace($"Loaded [{typeof(T).Name}] {path} in {watch.Elapsed.TotalMilliseconds:F1} ms.");

                if(errorMsg != null)
                {
                    Debug.Error($"Error loading content [{typeof(T).Name}] {path}: {errorMsg}");
                }
                return obj as T;
            }
            else
            {
                Debug.Error($"Failed to find content loader for type: {typeof(T).FullName} (trying to load '{path}').");
                return default;
            }
        }

        public bool HasLoaderFor<T>() where T : class
        {
            return HasLoaderFor(typeof(T));
        }

        public bool HasLoaderFor(Type t)
        {
            if (t == null)
                return false;

            return loaders.ContainsKey(t);
        }

        public void Dispose()
        {
            foreach (var pair in loaders)
            {
                pair.Value.Content = null;
            }
            loaders.Clear();
        }        
    }
}
