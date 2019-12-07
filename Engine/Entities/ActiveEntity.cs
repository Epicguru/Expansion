
using Engine.IO;
using Engine.Pathing;
using Engine.Sprites;
using Engine.Tasks;
using Engine.Threading;
using Engine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Engine.Entities
{
    /// <summary>
    /// An active entity is an entity that can perform tasks. This tends to be 'intelligent' pawns
    /// such as humans, animals, robots or perhaps vehicles. Active entities also follow a basic velocity and mass
    /// model, where all entities have a velocity, measured in meters per second, and a mass, measured in kilograms.
    /// Note that in-game each tile is considered to be 1 meter.
    /// </summary>
    public abstract class ActiveEntity : Entity
    {
        public readonly TaskManager TaskManager;
        public Task CurrentTask { get { return TaskManager.CurrentTask; } }
        /// <summary>
        /// The velocity vector of this entity, measured in meters per second. Each meter is a tile.
        /// </summary>
        public Vector2 Velocity;
        /// <summary>
        /// The mass, measured in kilograms, of this entity. Defaults to 50 kg.
        /// </summary>
        public float Mass
        {
            get
            {
                return _mass;
            }
            set
            {
                if (value < 0f)
                    _mass = 0f;
                _mass = value;
            }
        }
        /// <summary>
        /// The current path. See <see cref="Pathfinding"/> for pathfnding info, and <see cref="DoPathMovement"/>
        /// to enable or disable automatic path following.
        /// </summary>
        public List<PNode> CurrentPath { get; protected set; } = null;
        public bool IsPlottingPath { get { return realPathReq != null; } }
        public bool DoPathMovement { get; protected set; } = true;

        private ThreadedRequest<PathfindingRequest, PathfindingResult> realPathReq;
        private float _mass = 50f;

        public ActiveEntity(string name) : base(name, true)
        {
            TaskManager = new TaskManager();
        }

        internal override void InternalUpdate()
        {
            base.InternalUpdate();
            TaskManager.Update(this);
            Position += Velocity * Tile.SIZE * Time.deltaTime;
        }

        public void AddTask(Task t)
        {
            TaskManager.AddTask(t);
        }

        public void PlotPath(int destX, int destY)
        {
            this.PlotPath(new Point(destX, destY));
        }

        /// <summary>
        /// Stops moving along the current path, and disposes of that path.
        /// </summary>
        /// <param name="cancelPlot">If true then any path request that is currently active is also cancelled.</param>
        public void CancelPathMovement(bool cancelPlot = true)
        {
            CurrentPath = null;
            if(cancelPlot)
                CancelPathPlot();
        }

        public void PlotPath(Point destination)
        {
            if (IsPlottingPath)
            {
                CancelPathPlot();
            }
            var tp = base.TilePosition;
            PathfindingRequest req = new PathfindingRequest(tp.X, tp.Y, destination.X, destination.Y, PathCallback);
            realPathReq = JEngine.Pathfinding.Post(req);
        }

        public void CancelPathPlot()
        {
            if (!IsPlottingPath)
            {
                Debug.Warn($"Cannot cancel path plotting, there is no current request. See Entity.IsPlottingPath.");
                return;
            }

            realPathReq.Cancel();
        }

        private void PathCallback(ThreadedRequestResult requestResult, PathfindingResult pathResult)
        {
            if (requestResult == ThreadedRequestResult.Cancelled)
                return;

            if (requestResult == ThreadedRequestResult.Run)
                CurrentPath = pathResult.Path;

            realPathReq = null;

            OnPathfindingReturn(requestResult, pathResult);
        }

        protected virtual void OnPathfindingReturn(ThreadedRequestResult requestResult, PathfindingResult pathResult)
        {

        }

        /// <summary>
        /// Writes velocity and mass.
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(IOWriter writer)
        {
            base.Serialize(writer);

            writer.Write(Velocity);
            writer.Write(Mass);
        }

        /// <summary>
        /// Reads velocity and mass.
        /// </summary>
        /// <param name="reader"></param>
        public override void Deserialize(IOReader reader)
        {
            base.Deserialize(reader);

            Velocity = reader.ReadVector2();
            Mass = reader.ReadSingle();
        }

        public void DrawDebugPath(SpriteBatch spr)
        {
            if (CurrentPath == null)
                return;

            Color c = Color.MediumPurple;
            c.A = 160;
            Sprite tex = JEngine.Pixel;

            for (int i = 0; i < CurrentPath.Count; i++)
            {
                PNode point = CurrentPath[i];
                Rectangle pos = new Rectangle(point.X * Tile.SIZE, point.Y * Tile.SIZE, Tile.SIZE, Tile.SIZE);

                spr.Draw(tex, pos, c);
            }
        }
    }
}
