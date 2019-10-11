using System;

namespace Engine
{
    public abstract class ContentLoader
    {
        public readonly Type TargetType;

        public ContentLoader(Type type)
        {
            this.TargetType = type ?? throw new ArgumentNullException("type", "Type for a loader cannot be null!");
        }

        public abstract object Load(string path, out string error, params object[] args);
    }
}
