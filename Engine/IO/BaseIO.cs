namespace Engine.IO
{
    public class BaseIO
    {
        public bool IsForNetwork { get; }

        public BaseIO(bool forNet)
        {
            this.IsForNetwork = forNet;
        }
    }
}
