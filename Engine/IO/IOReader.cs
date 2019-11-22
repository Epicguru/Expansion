using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace Engine.IO
{
    public class IOReader : BaseIO
    {
        public NetIncomingMessage NetMessage { get; }
        public BinaryReader BinReader { get; }

        public IOReader(NetIncomingMessage msg) : base(true)
        {
            NetMessage = msg ?? throw new ArgumentNullException(nameof(msg));
        }

        public IOReader(BinaryReader reader) : base(false)
        {
            BinReader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public int ReadInt()
        {
            if (IsForNetwork)
                return NetMessage.ReadInt32();
            else
                return BinReader.ReadInt32();
        }

        public long ReadLong()
        {
            if (IsForNetwork)
                return NetMessage.ReadInt64();
            else
                return BinReader.ReadInt64();
        }

        public short ReadShort()
        {
            if (IsForNetwork)
                return NetMessage.ReadInt16();
            else
                return BinReader.ReadInt16();
        }

        public uint ReadUInt()
        {
            if (IsForNetwork)
                return NetMessage.ReadUInt32();
            else
                return BinReader.ReadUInt32();
        }

        public ushort ReadUShort()
        {
            if (IsForNetwork)
                return NetMessage.ReadUInt16();
            else
                return BinReader.ReadUInt16();
        }

        public ulong ReadULong()
        {
            if (IsForNetwork)
                return NetMessage.ReadUInt64();
            else
                return BinReader.ReadUInt64();
        }

        public bool ReadBool()
        {
            if (IsForNetwork)
                return NetMessage.ReadBoolean();
            else
                return BinReader.ReadBoolean();
        }

        public float ReadFloat()
        {
            if (IsForNetwork)
                return NetMessage.ReadFloat();
            else
                return BinReader.ReadSingle();
        }

        public double ReadDouble()
        {
            if (IsForNetwork)
                return NetMessage.ReadDouble();
            else
                return BinReader.ReadDouble();
        }

        public string ReadString()
        {
            if (IsForNetwork)
                return NetMessage.ReadString();
            else
                return BinReader.ReadString();
        }

        public byte[] ReadBytes(int count)
        {
            if (IsForNetwork)
                return NetMessage.ReadBytes(count);
            else
                return BinReader.ReadBytes(count);
        }

        public byte ReadByte()
        {
            if (IsForNetwork)
                return NetMessage.ReadByte();
            else
                return BinReader.ReadByte();
        }

        public sbyte ReadSByte()
        {
            if (IsForNetwork)
                return NetMessage.ReadSByte();
            else
                return BinReader.ReadSByte();
        }

        public Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Point ReadPoint()
        {
            return new Point(ReadInt(), ReadInt());
        }

        public Bounds ReadBounds()
        {
            return new Bounds(ReadVector2(), ReadVector2());
        }

        // TODO read entities from networking system.
    }
}
