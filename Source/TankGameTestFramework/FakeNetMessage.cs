using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace TankGameTestFramework
{
    public class FakeNetMessage
    {
        List<byte> _data = new List<byte>();

        public double SendTime { get; set; }

        public byte[] Data
        {
            get { return _data.ToArray(); }
            set { _data = value.ToList(); }
        }

        public long Position { get; set; }

        #region Not Implemented
        public int LengthBits
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int LengthBytes
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int PositionInBytes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool PeekBoolean()
        {
            throw new NotImplementedException();
        }

        public byte PeekByte()
        {
            throw new NotImplementedException();
        }

        public byte PeekByte(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public byte[] PeekBytes(int numberOfBytes)
        {
            throw new NotImplementedException();
        }

        public void PeekBytes(byte[] into, int offset, int numberOfBytes)
        {
            throw new NotImplementedException();
        }

        public byte[] PeekDataBuffer()
        {
            throw new NotImplementedException();
        }

        public double PeekDouble()
        {
            throw new NotImplementedException();
        }

        public float PeekFloat()
        {
            throw new NotImplementedException();
        }

        public short PeekInt16()
        {
            throw new NotImplementedException();
        }

        public int PeekInt32()
        {
            throw new NotImplementedException();
        }

        public int PeekInt32(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public long PeekInt64()
        {
            throw new NotImplementedException();
        }

        public long PeekInt64(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public sbyte PeekSByte()
        {
            throw new NotImplementedException();
        }

        public float PeekSingle()
        {
            throw new NotImplementedException();
        }

        public string PeekString()
        {
            throw new NotImplementedException();
        }

        public ushort PeekUInt16()
        {
            throw new NotImplementedException();
        }

        public uint PeekUInt32()
        {
            throw new NotImplementedException();
        }

        public uint PeekUInt32(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public ulong PeekUInt64()
        {
            throw new NotImplementedException();
        }

        public ulong PeekUInt64(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public void ReadAllFields(object target)
        {
            throw new NotImplementedException();
        }

        public void ReadAllFields(object target, BindingFlags flags)
        {
            throw new NotImplementedException();
        }

        public void ReadAllProperties(object target)
        {
            throw new NotImplementedException();
        }

        public void ReadAllProperties(object target, BindingFlags flags)
        {
            throw new NotImplementedException();
        }

        public void ReadBits(byte[] into, int offset, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public bool ReadBoolean()
        {
            throw new NotImplementedException();
        }

        public byte ReadByte()
        {
            throw new NotImplementedException();
        }

        public byte ReadByte(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public bool ReadByte(out byte result)
        {
            throw new NotImplementedException();
        }

        public bool ReadBytes(int numberOfBytes, out byte[] result)
        {
            throw new NotImplementedException();
        }

        public void ReadBytes(byte[] into, int offset, int numberOfBytes)
        {
            throw new NotImplementedException();
        }

        public double ReadDouble()
        {
            throw new NotImplementedException();
        }

        public float ReadFloat()
        {
            throw new NotImplementedException();
        }

        public short ReadInt16()
        {
            throw new NotImplementedException();
        }

        public int ReadInt32(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public bool ReadInt32(out int result)
        {
            throw new NotImplementedException();
        }

        public long ReadInt64()
        {
            throw new NotImplementedException();
        }

        public long ReadInt64(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public IPEndPoint ReadIpEndPoint()
        {
            throw new NotImplementedException();
        }

        public void ReadPadBits()
        {
            throw new NotImplementedException();
        }

        public int ReadRangedInteger(int min, int max)
        {
            throw new NotImplementedException();
        }

        public long ReadRangedInteger(long min, long max)
        {
            throw new NotImplementedException();
        }

        public float ReadRangedSingle(float min, float max, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public sbyte ReadSByte()
        {
            throw new NotImplementedException();
        }

        public float ReadSignedSingle(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public float ReadSingle()
        {
            throw new NotImplementedException();
        }

        public bool ReadSingle(out float result)
        {
            throw new NotImplementedException();
        }

        public string ReadString()
        {
            throw new NotImplementedException();
        }

        public bool ReadString(out string result)
        {
            throw new NotImplementedException();
        }

        public double ReadTime(bool highPrecision)
        {
            throw new NotImplementedException();
        }

        public double ReadTime(NetConnection connection, bool highPrecision)
        {
            throw new NotImplementedException();
        }

        public ushort ReadUInt16()
        {
            throw new NotImplementedException();
        }

        public uint ReadUInt32()
        {
            throw new NotImplementedException();
        }

        public uint ReadUInt32(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public bool ReadUInt32(out uint result)
        {
            throw new NotImplementedException();
        }

        public ulong ReadUInt64()
        {
            throw new NotImplementedException();
        }

        public ulong ReadUInt64(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public float ReadUnitSingle(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public int ReadVariableInt32()
        {
            throw new NotImplementedException();
        }

        public long ReadVariableInt64()
        {
            throw new NotImplementedException();
        }

        public uint ReadVariableUInt32()
        {
            throw new NotImplementedException();
        }

        public bool ReadVariableUInt32(out uint result)
        {
            throw new NotImplementedException();
        }

        public ulong ReadVariableUInt64()
        {
            throw new NotImplementedException();
        }

        public void SkipPadBits()
        {
            throw new NotImplementedException();
        }

        public void SkipPadBits(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public void Write(ushort source)
        {
            throw new NotImplementedException();
        }

        public void Write(float source)
        {
            throw new NotImplementedException();
        }

        public void Write(IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public void Write(short source)
        {
            throw new NotImplementedException();
        }

        public void Write(string source)
        {
            throw new NotImplementedException();
        }

        public void Write(uint source)
        {
            throw new NotImplementedException();
        }

        public void Write(sbyte source)
        {
            throw new NotImplementedException();
        }

        public void Write(INetBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public void Write(double source)
        {
            throw new NotImplementedException();
        }

        public void Write(ulong source, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public void Write(int source, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public void Write(byte source, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public void Write(uint source, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public void Write(long source, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public void Write(ushort source, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] source, int offsetInBytes, int numberOfBytes)
        {
            throw new NotImplementedException();
        }

        public void WriteAllFields(object ob)
        {
            throw new NotImplementedException();
        }

        public void WriteAllFields(object ob, BindingFlags flags)
        {
            throw new NotImplementedException();
        }

        public void WriteAllProperties(object ob)
        {
            throw new NotImplementedException();
        }

        public void WriteAllProperties(object ob, BindingFlags flags)
        {
            throw new NotImplementedException();
        }

        public void WriteAt(int offset, int source)
        {
            throw new NotImplementedException();
        }

        public void WriteAt(int offset, byte source)
        {
            throw new NotImplementedException();
        }

        public void WriteAt(int offset, short source)
        {
            throw new NotImplementedException();
        }

        public void WriteAt(int offset, uint source)
        {
            throw new NotImplementedException();
        }

        public void WriteAt(int offset, ushort source)
        {
            throw new NotImplementedException();
        }

        public void WriteAt(int offset, ulong source)
        {
            throw new NotImplementedException();
        }

        public void WritePadBits()
        {
            throw new NotImplementedException();
        }

        public void WritePadBits(int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public int WriteRangedInteger(int min, int max, int value)
        {
            throw new NotImplementedException();
        }

        public int WriteRangedInteger(long min, long max, long value)
        {
            throw new NotImplementedException();
        }

        public void WriteRangedSingle(float value, float min, float max, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public void WriteSignedSingle(float value, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public void WriteTime(bool highPrecision)
        {
            throw new NotImplementedException();
        }

        public void WriteTime(double localTime, bool highPrecision)
        {
            throw new NotImplementedException();
        }

        public void WriteUnitSingle(float value, int numberOfBits)
        {
            throw new NotImplementedException();
        }

        public int WriteVariableInt32(int value)
        {
            throw new NotImplementedException();
        }

        public int WriteVariableInt64(long value)
        {
            throw new NotImplementedException();
        }

        public int WriteVariableUInt32(uint value)
        {
            throw new NotImplementedException();
        }

        public int WriteVariableUInt64(ulong value)
        {
            throw new NotImplementedException();
        }
        #endregion

        public byte[] ReadBytes(int numberOfBytes)
        {
            byte[] bytes = new byte[numberOfBytes];
            Array.Copy(Data, Position, bytes, 0, numberOfBytes);
            Position += numberOfBytes;
            return bytes;
        }

        public int ReadInt32()
        {
            int result = BitConverter.ToInt32(Data, (int)Position);
            Position += sizeof(int);
            return result;
        }

        public void Write(long source)
        {
            _data.AddRange(BitConverter.GetBytes(source));
            Position += sizeof(long);
        }

        public void Write(byte[] source)
        {
            _data.AddRange(source);
            Position += sizeof(byte) * source.Length;
        }

        public void Write(bool value)
        {
            _data.AddRange(BitConverter.GetBytes(value));
            Position += sizeof(bool);
        }

        public void Write(byte source)
        {
            _data.AddRange(BitConverter.GetBytes(source));
            Position += sizeof(byte);
        }

        public void Write(ulong source)
        {
            _data.AddRange(BitConverter.GetBytes(source));
            Position += sizeof(ulong);
        }

        public void Write(int source)
        {
            _data.AddRange(BitConverter.GetBytes(source));
            Position += sizeof(int);
        }

        
    }
}