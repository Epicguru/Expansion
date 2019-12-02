using Engine.Threading;

namespace Engine.Pathing
{
    public class Pathfinding : ThreadCallbackManager<PathfindingRequest, PathfindingResult>
    {
        public Pathfinding(int threadCount) : base(threadCount)
        {

        }

        public ThreadedRequest<PathfindingRequest, PathfindingResult> Post(PathfindingRequest request)
        {
            var realReq = ThreadedRequest<PathfindingRequest, PathfindingResult>.Create(request.UponProcessed, request);
            base.Post(realReq);
            return realReq;
        }

        public override IThreadProcessor<PathfindingRequest, PathfindingResult> CreateProcessor(int threadIndex)
        {
            return new PathProcessor();
        }
    }
}
