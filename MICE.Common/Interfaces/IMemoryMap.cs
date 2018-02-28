﻿using System;

namespace MICE.Common.Interfaces
{
    /// <summary>
    /// Interface that represents memory that is mapped to various memory segments.
    /// </summary>
    public interface IMemoryMap
    {
        ushort ReadShort(int index);
        byte ReadByte(int index);
        void Write(int index, byte value);
    }
}
