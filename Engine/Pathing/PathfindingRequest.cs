using Engine.Threading;
using System;

namespace Engine.Pathing
{
    public struct PathfindingRequest
    {
        public int StartX, StartY;
        public int EndX, EndY;
        public Action<ThreadedRequestResult, PathfindingResult> UponProcessed;

        public PathfindingRequest(int startX, int startY, int endX, int endY, Action<ThreadedRequestResult, PathfindingResult> uponProcessed)
        {
            this.StartX = startX;
            this.StartY = startY;
            this.EndX = endX;
            this.EndY = endY;
            this.UponProcessed = uponProcessed;
        }
    }
}
