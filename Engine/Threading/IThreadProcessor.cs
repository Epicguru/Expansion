
namespace Engine.Threading
{
    public interface IThreadProcessor<In, Out>
    {
        Out Process(In input);
    }
}
