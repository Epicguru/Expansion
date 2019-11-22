using Lidgren.Network;
using Microsoft.Xna.Framework;
using System.IO;

namespace Engine.IO
{
    public class IOWriter : BaseIO
    {
        public NetOutgoingMessage NetMessage { get; }
        public BinaryWriter BinWriter { get; }

        public IOWriter(NetOutgoingMessage msg) : base(true)
        {
            NetMessage = msg;
        }

        public IOWriter(BinaryWriter writer) : base(false)
        {
            BinWriter = writer;
        }

        public void Write(int int32)
        {
            if (IsForNetwork)
                NetMessage.Write(int32);
            else
                BinWriter.Write(int32);
        }

        public void Write(long int64)
        {
            if (IsForNetwork)
                NetMessage.Write(int64);
            else
                BinWriter.Write(int64);
        }

        public void Write(short int16)
        {
            if (IsForNetwork)
                NetMessage.Write(int16);
            else
                BinWriter.Write(int16);
        }

        public void Write(uint uInt32)
        {
            if (IsForNetwork)
                NetMessage.Write(uInt32);
            else
                BinWriter.Write(uInt32);
        }

        public void Write(ushort uInt16)
        {
            if (IsForNetwork)
                NetMessage.Write(uInt16);
            else
                BinWriter.Write(uInt16);
        }

        public void Write(ulong uInt64)
        {
            if (IsForNetwork)
                NetMessage.Write(uInt64);
            else
                BinWriter.Write(uInt64);
        }

        public void Write(bool boolean)
        {
            if (IsForNetwork)
                NetMessage.Write(boolean);
            else
                BinWriter.Write(boolean);
        }

        public void Write(float single32)
        {
            if (IsForNetwork)
                NetMessage.Write(single32);
            else
                BinWriter.Write(single32);
        }

        public void Write(double single64)
        {
            if (IsForNetwork)
                NetMessage.Write(single64);
            else
                BinWriter.Write(single64);
        }

        public void Write(string str)
        {
            if (IsForNetwork)
                NetMessage.Write(str);
            else
                BinWriter.Write(str);
        }

        public void Write(byte b)
        {
            if (IsForNetwork)
                NetMessage.Write(b);
            else
                BinWriter.Write(b);
        }

        public void Write(sbyte sb)
        {
            if (IsForNetwork)
                NetMessage.Write(sb);
            else
                BinWriter.Write(sb);
        }

        public void Write(Vector2 v2)
        {
            Write(v2.X);
            Write(v2.Y);
        }

        public void Write(Vector3 v3)
        {
            Write(v3.X);
            Write(v3.Y);
            Write(v3.Z);
        }

        public void Write(Vector4 v4)
        {
            Write(v4.X);
            Write(v4.Y);
            Write(v4.Z);
            Write(v4.W);
        }

        public void Write(Point point)
        {
            Write(point.X);
            Write(point.Y);
        }

        public void Write(Bounds bounds)
        {
            Write(bounds.Pos);
            Write(bounds.Size);
        }
    }
}
