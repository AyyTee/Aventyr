using System.Net;
using System.Reflection;

namespace Lidgren.Network
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3001 // Argument type is not CLS-compliant
#pragma warning disable CS3002 // Return type is not CLS-compliant
#pragma warning disable CS3006 // Overloaded method differing only in ref or out, or in array rank, is not CLS-compliant
    public interface INetBuffer
    {
        byte[] Data { get; set; }
        int LengthBits { get; set; }
        int LengthBytes { get; set; }
        long Position { get; set; }
        int PositionInBytes { get; }

        void EnsureBufferSize(int numberOfBits);
        bool PeekBoolean();
        byte PeekByte();
        byte PeekByte(int numberOfBits);
        byte[] PeekBytes(int numberOfBytes);
        void PeekBytes(byte[] into, int offset, int numberOfBytes);
        byte[] PeekDataBuffer();
        double PeekDouble();
        float PeekFloat();
        short PeekInt16();
        int PeekInt32();
        int PeekInt32(int numberOfBits);
        long PeekInt64();
        long PeekInt64(int numberOfBits);
        sbyte PeekSByte();
        float PeekSingle();
        string PeekString();
        ushort PeekUInt16();
        uint PeekUInt32();
        uint PeekUInt32(int numberOfBits);
        ulong PeekUInt64();
        ulong PeekUInt64(int numberOfBits);
        void ReadAllFields(object target);
        void ReadAllFields(object target, BindingFlags flags);
        void ReadAllProperties(object target);
        void ReadAllProperties(object target, BindingFlags flags);
        void ReadBits(byte[] into, int offset, int numberOfBits);
        bool ReadBoolean();
        byte ReadByte();
        bool ReadByte(out byte result);
        byte ReadByte(int numberOfBits);
        byte[] ReadBytes(int numberOfBytes);
        bool ReadBytes(int numberOfBytes, out byte[] result);
        void ReadBytes(byte[] into, int offset, int numberOfBytes);
        double ReadDouble();
        float ReadFloat();
        short ReadInt16();
        int ReadInt32();
        bool ReadInt32(out int result);
        int ReadInt32(int numberOfBits);
        long ReadInt64();
        long ReadInt64(int numberOfBits);
        IPEndPoint ReadIPEndPoint();
        void ReadPadBits();
        long ReadRangedInteger(long min, long max);
        int ReadRangedInteger(int min, int max);
        float ReadRangedSingle(float min, float max, int numberOfBits);
        sbyte ReadSByte();
        float ReadSignedSingle(int numberOfBits);
        float ReadSingle();
        bool ReadSingle(out float result);
        string ReadString();
        bool ReadString(out string result);
        double ReadTime(NetConnection connection, bool highPrecision);
        ushort ReadUInt16();
        uint ReadUInt32();
        bool ReadUInt32(out uint result);
        uint ReadUInt32(int numberOfBits);
        ulong ReadUInt64();
        ulong ReadUInt64(int numberOfBits);
        float ReadUnitSingle(int numberOfBits);
        int ReadVariableInt32();
        long ReadVariableInt64();
        uint ReadVariableUInt32();
        bool ReadVariableUInt32(out uint result);
        ulong ReadVariableUInt64();
        void SkipPadBits();
        void SkipPadBits(int numberOfBits);
        void Write(double source);
        void Write(INetBuffer buffer);
        void Write(sbyte source);
        void Write(string source);
        void Write(uint source);
        void Write(short source);
        void Write(ulong source);
        void Write(int source);
        void Write(ushort source);
        void Write(float source);
        void Write(IPEndPoint endPoint);
        void Write(long source);
        void Write(byte[] source);
        void Write(byte source);
        void Write(bool value);
        void Write(ushort source, int numberOfBits);
        void Write(long source, int numberOfBits);
        void Write(uint source, int numberOfBits);
        void Write(ulong source, int numberOfBits);
        void Write(int source, int numberOfBits);
        void Write(byte source, int numberOfBits);
        void Write(byte[] source, int offsetInBytes, int numberOfBytes);
        void WriteAllFields(object ob);
        void WriteAllFields(object ob, BindingFlags flags);
        void WriteAllProperties(object ob);
        void WriteAllProperties(object ob, BindingFlags flags);
        void WriteAt(int offset, ulong source);
        void WriteAt(int offset, ushort source);
        void WriteAt(int offset, short source);
        void WriteAt(int offset, uint source);
        void WriteAt(int offset, int source);
        void WriteAt(int offset, byte source);
        void WritePadBits();
        void WritePadBits(int numberOfBits);
        int WriteRangedInteger(long min, long max, long value);
        int WriteRangedInteger(int min, int max, int value);
        void WriteRangedSingle(float value, float min, float max, int numberOfBits);
        void WriteSignedSingle(float value, int numberOfBits);
        void WriteTime(bool highPrecision);
        void WriteTime(double localTime, bool highPrecision);
        void WriteUnitSingle(float value, int numberOfBits);
        int WriteVariableInt32(int value);
        int WriteVariableInt64(long value);
        int WriteVariableUInt32(uint value);
        int WriteVariableUInt64(ulong value);
    }
#pragma warning restore CS3006 // Overloaded method differing only in ref or out, or in array rank, is not CLS-compliant
#pragma warning restore CS3002 // Return type is not CLS-compliant
#pragma warning restore CS3001 // Argument type is not CLS-compliant
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}