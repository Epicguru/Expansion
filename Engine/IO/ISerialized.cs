using System.IO;

namespace Engine.IO
{
    public interface ISerialized
    {
        void Serialize(IOWriter writer);
        void Deserialize(IOReader reader);
    }
}
