using Engine.Entities;
using Microsoft.Xna.Framework;

namespace Engine.Tasks
{
    public class Task_PlotPath : Task
    {
        public int DestinationX { get; }
        public int DestinationY { get; }

        public bool hasStarted = false;
        private ActiveEntity e;

        public Task_PlotPath(int destX, int destY) : base("Plot Path")
        {
            DestinationX = destX;
            DestinationY = destY;
            Description = "Planning path.";
        }

        protected override void Update(ActiveEntity e)
        {
            this.e = e;
            if(!hasStarted)
            {
                e.PlotPath(new Point(DestinationX, DestinationY));
                hasStarted = true;
            }
            else
            {
                if(!e.IsPlottingPath)
                {
                    Complete();
                    e = null;
                }
            }
        }

        protected override void OnCancel(ActiveEntity e)
        {
            if(e.IsPlottingPath)
                e.CancelPathPlot();
            e = null;
        }
    }
}
