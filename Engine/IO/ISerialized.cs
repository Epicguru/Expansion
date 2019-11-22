using System.IO;

namespace Engine.IO
{
    public interface ISerialized
    {
        void Serialize(IOWriter writer, bool forLoad);
        void Deserialize(IOReader reader, bool forLoad);
    }
}
