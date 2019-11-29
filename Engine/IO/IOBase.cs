namespace Engine.IO
{
    public interface IOBase
    {
        int Length { get; }

        void StartPadding();

        void EndPadding(int totalSize);
    }
}
