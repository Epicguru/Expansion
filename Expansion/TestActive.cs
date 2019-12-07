using Engine;
using Engine.Entities;
using Engine.Pathing;
using Engine.Tasks;
using Engine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Expansion
{
    public class TestActive : ActiveEntity
    {
        public TestActive() : base("Test Active Entity")
        {
            Size = new Vector2(8, 16);
        }

        public (Vector2 pos, int index) GetClosestPathPoint()
        {
            if(CurrentPath.Count == 1)
            {
                PNode point = CurrentPath[0];
                Vector2 pos = new Vector2(point.X + 0.5f, point.Y + 0.5f) * Tile.SIZE;

                return (pos, 0);
            }

            float lowest = float.MaxValue;
            Vector2 lowestPos = Vector2.Zero;
            int lowestIndex = -1;
            for (int i = 0; i < CurrentPath.Count; i++)
            {
                PNode point = CurrentPath[i];
                Vector2 pos = new Vector2(point.X + 0.5f, point.Y + 0.5f) * Tile.SIZE;

                float sqrDst = Vector2.DistanceSquared(pos, Center);
                if (sqrDst < lowest)
                {
                    lowest = sqrDst;
                    lowestIndex = i;
                    lowestPos = pos;
                }
            }

            return (lowestPos, lowestIndex);
        }

        public override void Update()
        {
            if (Input.KeyDown(Keys.Space))
            {
                base.AddTask(new Task_ChopTree(Input.MouseWorldTilePos.X, Input.MouseWorldTilePos.Y));
            }

            Velocity = Vector2.Zero;

            if(CurrentPath != null)
            {
                (Vector2 nextPos, int nextIndex) = GetClosestPathPoint();

                Vector2 direction = (nextPos - Center);
                Velocity += direction.GetNormalized() * 5f;
                if(direction.LengthSquared() <= Tile.SIZE * 0.5f)
                {
                    for (int i = 0; i < nextIndex + 1; i++)
                    {
                        CurrentPath.RemoveAt(0);
                    }
                    if (CurrentPath.Count == 0)
                        CurrentPath = null;
                }
            }
            if(CurrentPath == null)
            {
                //PlotPath(TilePosition.X + Rand.Range(-10, 11), TilePosition.Y + Rand.Range(-10, 11));
            }
        }

        public override void Draw(SpriteBatch spr)
        {
            spr.Draw(BaseScreen.Character, Position, Color.White);
        }
    }
}
