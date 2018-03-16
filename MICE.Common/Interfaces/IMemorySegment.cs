using System;

namespace MICE.Common.Interfaces
{
    /// <summary>
    /// Inteferface that represents a segment of memory.
    /// </summary>
    public interface IMemorySegment
    {
        int LowerIndex { get; }
        int UpperIndex { get; }
        string Name { get; }

        bool IsIndexInRange(int index);
        bool ContainsIndex(int index);
        int GetOffsetInSegment(int index);

        (int min, int max) GetRange();
        byte ReadByte(int index);
        ushort ReadShort(int index);

        void Write(int index, byte value);
        void Write(int index, ushort value);
        void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length);

        Action<int, byte> AfterReadAction { get; }
        Action<int, byte> AfterWriteAction { get; }
    }
}
