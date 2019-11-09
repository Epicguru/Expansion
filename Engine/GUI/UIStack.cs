using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Engine.GUI
{
    internal class UIStack
    {
        internal struct StackObject
        {
            public Rectangle TotalBounds;
            public Point Position;
            public UIFlow Flow;

            public StackObject(Rectangle r, Point p, UIFlow f)
            {
                TotalBounds = r;
                Position = p;
                Flow = f;
            }

            public void EncompassBounds(Rectangle otherBounds)
            {
                this.TotalBounds = Rectangle.Union(this.TotalBounds, otherBounds);
            }

            public void UpdateBounds(Rectangle newBounds)
            {
                this.TotalBounds = newBounds;
            }
        }

        private readonly List<StackObject> areas = new List<StackObject>();

        private StackObject Current { get { return areas[areas.Count - 1]; } }

        public UIFlow GetCurrentFlow()
        {
            return Current.Flow;
        }

        public Point GetCurrentPos()
        {
            return Current.Position;
        }

        public Rectangle GetCurrentBounds()
        {
            return Current.TotalBounds;
        }

        public void UpdateCurrentBounds(Rectangle newBounds)
        {
            var current = Current;
            current.UpdateBounds(newBounds);
            areas[areas.Count - 1] = current;
        }

        public void ExpandCurrentBounds(Rectangle newArea)
        {
            var current = Current;
            current.EncompassBounds(newArea);
            areas[areas.Count - 1] = current;
        }

        public void UpdateCurrentPos(Point newPos)
        {
            var current = Current;
            current.Position = newPos;
            areas[areas.Count - 1] = current;
        }

        public void Pop(bool updatePrevious)
        {
            var obj = Current;
            areas.RemoveAt(areas.Count - 1);
            if (updatePrevious)
            {
                ExpandCurrentBounds(obj.TotalBounds);
                UpdateCurrentPos(UILayout.GetUpdatedPoint(Current.Flow, Current.Position, obj.TotalBounds));
            }            
        }

        public void Push(Point pos, UIFlow flow)
        {
            areas.Add(new StackObject(new Rectangle(pos, Point.Zero), pos, flow));
        }

        public void Reset(Point pos, UIFlow flow)
        {
            areas.Clear();
            areas.Add(new StackObject(new Rectangle(pos, Point.Zero), pos, flow));
        }
    }
}
