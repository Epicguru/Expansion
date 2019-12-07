namespace Engine.Tasks
{
    public class Task_MoveTo : CompoundTask
    {
        public int DestinationX { get; }
        public int DestinationY { get; }

        public Task_MoveTo(int x, int y) : base("Move To")
        {
            Description = "Moving...";

            DestinationX = x;
            DestinationY = y;

            base.AddTask(new Task_PlotPath(x, y));
            base.AddTask(new Task_CompletePath());
        }
    }
}
