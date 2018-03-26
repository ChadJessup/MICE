using MICE.Common.Misc;
using System;

namespace MICE.Common.Interfaces
{
    /// <summary>
    /// Inteferface that represents a segment of memory.
    /// </summary>
    public interface IMemorySegment
    {
        Range<int> Range { get; }
        string Name { get; }

        bool IsIndexInRange(int index);
        bool ContainsIndex(int index);
        int GetOffsetInSegment(int index);

        Range<int> GetRange();
        byte ReadByte(int index);
        ushort ReadShort(int index);

        void Write(int index, byte value);
        void Write(int index, ushort value);
        void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length);

        Action<int, byte> AfterReadAction { get; }
        Action<int, byte> AfterWriteAction { get; }
    }
}
