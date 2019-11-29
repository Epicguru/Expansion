using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Engine.IO
{
    public class IOWriter : BinaryWriter, IOBase
    {
        public int Length { get { return (int)base.BaseStream.Length; } }
        public int PaddingDepth { get { return paddingLengths.Count; } }

        private Stack<int> paddingLengths = new Stack<int>();

        public IOWriter(Stream output) : base(output)
        {

        }

        public void StartPadding()
        {
            paddingLengths.Push(Length);
        }

        public void EndPadding(int totalSize)
        {
            int startLength = paddingLengths.Pop();
            int size = Length - startLength;

            if (size > totalSize)
                throw new System.IO.IOException($"Size of {size} bytes exceeds the padding size {totalSize}.");

            int required = totalSize - size;
            Write(IOUtils.GetArray(required));
        }

        public void Write(ISerialized serializable)
        {
            if (serializable == null)
                throw new System.ArgumentNullException(nameof(serializable));

            serializable.Serialize(this);
        }

        public void Write(Vector2 vector2)
        {
            this.Write(vector2.X);
            this.Write(vector2.Y);
        }

        public void Write(Vector3 vector3)
        {
            this.Write(vector3.X);
            this.Write(vector3.Y);
            this.Write(vector3.Z);
        }

        public void Write(Vector4 vector4)
        {
            this.Write(vector4.X);
            this.Write(vector4.Y);
            this.Write(vector4.Z);
            this.Write(vector4.W);
        }

        public void Write(Bounds bounds)
        {
            this.Write(bounds.Pos);
            this.Write(bounds.Size);
        }

        public void Write(Point point)
        {
            this.Write(point.X);
            this.Write(point.Y);
        }

        public void Write(Point3D point3D)
        {
            this.Write(point3D.X);
            this.Write(point3D.Y);
            this.Write(point3D.Z);
        }

    }
}
