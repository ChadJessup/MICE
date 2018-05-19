using System;
using System.Collections.Generic;

namespace MICE.Common.Interfaces
{
    /// <summary>
    /// Interface that represents memory that is mapped to various memory segments.
    /// </summary>
    public interface IMemoryMap : ICollection<IMemorySegment>
    {
        byte[] Data { get; }
        ushort ReadShort(int index);
        byte ReadByte(int index);
        void Write(int index, byte value);
        T GetMemorySegment<T>(string segmentName) where T : IMemorySegment;
        IEnumerable<IMemorySegment> GetMemorySegments();
        IEnumerable<T> GetMemorySegments<T>() where T : IMemorySegment;

        void BulkTransfer(ushort startAddress, Span<byte> destinationArray, int destinationIndex, int size);
    }
}
