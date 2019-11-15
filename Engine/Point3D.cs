using Microsoft.Xna.Framework;

namespace Engine
{
    public struct Point3D
    {
        public int X, Y, Z;

        public Point3D(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3D(Point point) : this(point, 0)
        {

        }

        public Point3D(Point point, int z)
        {
            X = point.X;
            Y = point.Y;
            Z = z;
        }

        public static Point3D operator +(Point3D a, Point3D b)
        {
            return new Point3D(b.X + a.X, b.Y + a.Y, b.Z + a.Z);
        }

        public static Point3D operator +(Point3D a, Point b)
        {
            return new Point3D(a.X + b.X, a.Y + b.Y, a.Z);
        }

        public static Point3D operator -(Point3D a, Point3D b)
        {
            return new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Point3D operator -(Point3D a, Point b)
        {
            return new Point3D(a.X - b.X, a.Y - b.Y, a.Z);
        }

        public static implicit operator Vector3(Point3D self)
        {
            return new Vector3(self.X, self.Y, self.Z);
        }

        public static implicit operator Point(Point3D self)
        {
            return new Point(self.X, self.Y);
        }
    }
}
