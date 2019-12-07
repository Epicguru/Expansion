
using Microsoft.Xna.Framework;
using System;

namespace Engine
{
    public struct Bounds : IEquatable<Bounds>
    {
        public Vector2 Pos;
        public Vector2 Size;

        public Vector2 Center { get { return Pos + Size * 0.5f; } }
        public float Left { get { return Pos.X; } }
        public float Top { get { return Pos.Y; } }
        public float Right { get { return Pos.X + Size.X; } }
        public float Bottom { get { return Pos.Y + Size.Y; } }

        public Bounds(Vector2 pos, Vector2 size)
        {
            this.Pos = pos;
            this.Size = size;
        }

        public Bounds(float x, float y, float width, float height)
        {
            this.Pos = new Vector2(x, y);
            this.Size = new Vector2(width, height);
        }

        public Bounds(Rectangle r) : this(r.Location.ToVector2(), r.Size.ToVector2())
        {

        }

        public bool Contains(Vector2 point)
        {
            return point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
        }

        public static bool Overlaps(Bounds a, Bounds b)
        {
            return a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;
        }

        public bool Overlaps(Bounds other)
        {
            return Overlaps(this, other);
        }

        public bool Equals(Bounds other)
        {
            return other.Pos == this.Pos && other.Size == this.Size;
        }

        /// <summary>
        /// Converts this Bounds to a rectangle. The rectangle's position will be a rounded version of <see cref="Pos"/>
        /// and it's size will be the ceiling of <see cref="Size"/>. For example, bounds [0, 11.4, 2.1, 7.6] will be converted to [0, 11, 3, 8].
        /// </summary>
        /// <returns>The aproximate rectangle representation.</returns>
        public Rectangle ToRectangle()
        {
            return new Rectangle(new Point((int)Math.Round(Pos.X), (int)Math.Round(Pos.Y)), new Point((int)Math.Ceiling(Size.X), (int)Math.Ceiling(Size.Y)));
        }

        public override string ToString()
        {
            return $"({Pos.X:F1}, {Pos.Y:F1}, {Size.X:F1}, {Size.Y:F1})";
        }
    }
}
