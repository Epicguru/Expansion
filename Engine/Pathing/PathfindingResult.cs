using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Engine.Pathing
{
    public struct PathfindingResult
    {
        public List<PNode> Path;
        public PathState Result;
    }

    public enum PathState
    {
        ERROR_INTERNAL,
        ERROR_START_NOT_WALKABLE,
        ERROR_END_NOT_WALKABLE,
        ERROR_START_IS_END,
        ERROR_PATH_TOO_LONG,
        SUCCESSFUL
    }
}
