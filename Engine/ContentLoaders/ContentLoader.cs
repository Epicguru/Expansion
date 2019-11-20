
using Microsoft.Xna.Framework.Content;
using System;

namespace Engine.ContentLoaders
{
    public abstract class ContentLoader : IDisposable
    {
        public Type TargetType { get; private set; }
        public JContent JContent { get; internal set; }

        public ContentLoader(Type targetType)
        {
            this.TargetType = targetType;
        }

        public abstract object Load(ContentManager content, string path);

        public abstract void Unload(object obj);

        public virtual void Dispose()
        {
            TargetType = null;
            JContent = null;
        }
    }
}
