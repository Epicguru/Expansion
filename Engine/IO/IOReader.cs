using Engine.Items;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Engine.IO
{
    public class IOReader : BinaryReader, IOBase
    {
        public int Length { get { return (int)base.BaseStream.Length; } }
        private static Stack<int> paddingPositions = new Stack<int>();

        public IOReader(Stream input) : base(input)
        {

        }       

        public void StartPadding()
        {
            paddingPositions.Push((int)base.BaseStream.Position);
        }

        public void EndPadding(int totalSize)
        {
            int startPos = paddingPositions.Pop();
            int readAmount = (int)BaseStream.Position - startPos;
            int remaining = totalSize - readAmount;

            if (remaining < 0)
                throw new System.IO.IOException($"Error in padding read: {readAmount} bytes have been read since pad start, but this exceeds the total expected pad size of {totalSize}.");

            // Read this way to avoid creating garbage byte arrays.
            for (int i = 0; i < remaining; i++)
            {
                base.ReadByte();
            }
        }

        public void ReadSerializable(ref ISerialized obj)
        {
            if (obj == null)
                throw new System.ArgumentNullException(nameof(obj), "The object to deserialize to cannot be null.");

            obj.Deserialize(this);
        }

        public Vector2 ReadVector2()
        {
            return new Vector2(this.ReadSingle(), this.ReadSingle());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(this.ReadSingle(), this.ReadSingle(), this.ReadSingle());
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(this.ReadSingle(), this.ReadSingle(), this.ReadSingle(), this.ReadSingle());
        }

        public Point ReadPoint()
        {
            return new Point(this.ReadInt32(), this.ReadInt32());
        }

        public Point3D ReadPoint3D()
        {
            return new Point3D(this.ReadInt32(), this.ReadInt32(), this.ReadInt32());
        }

        public Bounds ReadBounds()
        {
            return new Bounds(this.ReadVector2(), this.ReadVector2());
        }

        public ItemStack ReadItemStack()
        {
            ushort id = ReadUInt16();
            int count = ReadInt32();

            return new ItemStack(id, count);
        }
    }
}
