using MICE.Common.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MICE.Common.Misc
{
    public class MemoryMapper2 : IMemoryMap
    {
        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public byte[] Data => throw new NotImplementedException();

        public void Add(IMemorySegment item)
        {
            throw new NotImplementedException();
        }

        public void BulkTransfer(ushort startAddress, Span<byte> destinationArray, int destinationIndex, int size)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(IMemorySegment item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IMemorySegment[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IMemorySegment> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public T GetMemorySegment<T>(string segmentName) where T : IMemorySegment
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMemorySegment> GetMemorySegments()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetMemorySegments<T>() where T : IMemorySegment
        {
            throw new NotImplementedException();
        }

        public byte ReadByte(int index)
        {
            throw new NotImplementedException();
        }

        public ushort ReadShort(int index)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IMemorySegment item)
        {
            throw new NotImplementedException();
        }

        public void Write(int index, byte value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
