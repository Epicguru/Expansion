using Engine.Pathing.HSPQ;
using Engine.Threading;
using Engine.Tiles;
using System;
using System.Collections.Generic;

namespace Engine.Pathing
{
    public class PathProcessor : IThreadProcessor<PathfindingRequest, PathfindingResult>
    {
        public const int MAX = 1024;
        public const float ROOT_2 =      1.4142135f;
        public const float HALF_ROOT_2 = 0.5f * ROOT_2;

        private FastPriorityQueue<PNode> open = new FastPriorityQueue<PNode>(MAX);
        private Dictionary<PNode, PNode> cameFrom = new Dictionary<PNode, PNode>();
        private Dictionary<PNode, float> costSoFar = new Dictionary<PNode, float>();
        private List<(PNode node, float cost)> near = new List<(PNode, float)>();
        private bool left, right, below, above;

        public PathProcessor()
        {

        }

        public PathState Run(int startX, int startY, int endX, int endY, TileLayer map, out List<PNode> path)
        {
            if (map == null)
            {
                path = null;
                return PathState.ERROR_INTERNAL;
            }
            if (!map.GetWalkData(startX, startY).walkable)
            {
                path = null;
                return PathState.ERROR_START_NOT_WALKABLE;
            }
            if (!map.GetWalkData(endX, endY).walkable)
            {
                path = null;
                return PathState.ERROR_END_NOT_WALKABLE;
            }

            // Clear everything up.
            Clear();

            var start = new PNode(startX, startY);
            var end = new PNode(endX, endY);

            // Check the start/end relationship.
            if (start.Equals(end))
            {
                path = null;
                return PathState.ERROR_START_IS_END;
            }

            // Add the starting point to all relevant structures.
            open.Enqueue(start, 0f);
            cameFrom[start] = start;
            costSoFar[start] = 0f;

            int count;
            while ((count = open.Count) > 0)
            {
                // Detect if the current open amount exceeds the capacity.
                // This only happens in very large open areas. Corridors and hallways will never cause this, not matter how large the actual path length.
                if (count >= MAX - 8)
                {
                    path = null;

                    // Clear everything up.
                    Clear();

                    return PathState.ERROR_PATH_TOO_LONG;
                }

                var current = open.Dequeue();
                var currentCost = map.GetWalkData(current.X, current.Y).cost;

                if (current.Equals(end))
                {
                    // We found the end of the path!
                    path = TracePath(end);

                    // Clear everything up.
                    Clear();

                    return PathState.SUCCESSFUL;
                }

                // Get all neighbours (tiles that can be walked on to)
                var neighbours = GetNear(current, map);
                foreach (var pair in neighbours)
                {
                    PNode n = pair.node;
                    float newCost = costSoFar[current] + GetCost(current, pair.node, currentCost, pair.cost);

                    if (!costSoFar.ContainsKey(n) || newCost < costSoFar[n])
                    {
                        costSoFar[n] = newCost;
                        float priority = newCost + Heuristic(current, n);
                        open.Enqueue(n, priority);
                        cameFrom[n] = current;
                    }
                }
            }

            // Clear everything up.
            Clear();

            path = null;
            return PathState.ERROR_INTERNAL;
        }

        private List<PNode> TracePath(PNode end)
        {
            List<PNode> path = new List<PNode>();
            PNode child = end;

            bool run = true;
            while (run)
            {
                PNode previous = cameFrom[child];
                path.Add(child);
                if (previous != null && child != previous)
                {
                    child = previous;
                }
                else
                {
                    run = false;
                }
            }

            path.Reverse();
            return path;
        }

        public void Clear()
        {
            costSoFar.Clear();
            cameFrom.Clear();
            near.Clear();
            open.Clear();
        }

        private float Heuristic(PNode a, PNode b)
        {
            // Gives a rough distance.
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private float GetCost(PNode a, PNode b, float ca, float cb)
        {
            // Only intended for neighbours.

            // Is directly horzontal
            if (Math.Abs(a.X - b.X) == 1 && a.Y == b.Y)
            {
                return 0.5f * ca + 0.5f * cb;
            }

            // Directly vertical.
            if (Math.Abs(a.Y - b.Y) == 1 && a.X == b.X)
            {
                return 0.5f * ca + 0.5f * cb;
            }

            // Assume that it is on one of the corners.
            return HALF_ROOT_2 * ca + HALF_ROOT_2 * cb;
        }

        private List<(PNode node, float cost)> GetNear(PNode node, TileLayer map)
        {
            // Want to add nodes connected to the center node, if they are walkable.
            // This code stops the pathfinder from cutting corners, and going through walls that are diagonal from each other.

            near.Clear();
            (bool walkable, float cost) data;
            int x = node.X;
            int y = node.Y;

            // Left
            left = false;
            data = map.GetWalkData(x - 1, y);
            if (data.walkable)
            {
                near.Add((new PNode(node.X - 1, node.Y), data.cost));
                left = true;
            }

            // Right
            right = false;
            data = map.GetWalkData(x + 1, y);
            if (data.walkable)
            {
                near.Add((new PNode(node.X + 1, node.Y), data.cost));
                right = true;
            }

            // Above
            above = false;
            data = map.GetWalkData(x, y + 1);
            if (data.walkable)
            {
                near.Add((new PNode(node.X, node.Y + 1), data.cost));
                above = true;
            }

            // Below
            below = false;
            data = map.GetWalkData(x, y - 1);
            if (data.walkable)
            {
                near.Add((new PNode(node.X, node.Y - 1), data.cost));
                below = true;
            }

            // Above-Left
            if (left && above)
            {
                data = map.GetWalkData(x - 1, y + 1);
                if (data.walkable)
                {
                    near.Add((new PNode(node.X - 1, node.Y + 1), data.cost));
                }
            }

            // Above-Right
            if (right && above)
            {
                data = map.GetWalkData(x + 1, y + 1);
                if (data.walkable)
                {
                    near.Add((new PNode(node.X + 1, node.Y + 1), data.cost));
                }
            }

            // Below-Left
            if (left && below)
            {
                data = map.GetWalkData(x - 1, y - 1);
                if (data.walkable)
                {
                    near.Add((new PNode(node.X - 1, node.Y - 1), data.cost));
                }
            }

            // Below-Right
            if (right && below)
            {
                data = map.GetWalkData(x + 1, y - 1);
                if (data.walkable)
                {
                    near.Add((new PNode(node.X + 1, node.Y - 1), data.cost));
                }
            }

            return near;
        }

        public void Dispose()
        {
            Clear();
            costSoFar = null;
            cameFrom = null;
            near = null;
            open = null;
        }

        public PathfindingResult Process(PathfindingRequest input)
        {
            List<PNode> list = null;
            var state = Run(input.StartX, input.StartY, input.EndX, input.EndY, JEngine.TileMap, out list);

            return new PathfindingResult() { Path = list, Result = state };
        }
    }
}
