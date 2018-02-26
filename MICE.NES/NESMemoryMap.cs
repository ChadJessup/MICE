using MICE.Common.Interfaces;
using MICE.Common.Misc;
using MICE.Components.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace MICE.Nintendo
{
    /// <summary>
    /// A class that represents how NES memory is mapped out to various components.
    /// </summary>
    public class NESMemoryMap : IMemoryMap
    {
        private MemoryMapper MemoryMapper = new MemoryMapper();

        public NESMemoryMap()
        {
            // NES memory is mapped out like this - with some trickery possible in the ROM itself to further map out memory.
            // Details for those will be in the individual components.

            // http://nesdev.com/NESDoc.pdf - Figure 2-3 CPU memory map
            // RAM - $0000-$1FFF
            this.MemoryMapper.Add(new RAM(0x0000, 0x00ff, "Zero Page"));
            this.MemoryMapper.Add(new RAM(0x0100, 0x01ff, "Stack"));
            this.MemoryMapper.Add(new RAM(0x0200, 0x07ff, "RAM"));

            // $0800 - RAM - Mirrors $0000-$07FF
            this.MemoryMapper.Add(new MirroredMemory(0x0800, 0x0fff, 0x0000, 0x07ff, "Mirrored Ram #1"));
            this.MemoryMapper.Add(new MirroredMemory(0x1000, 0x17FF, 0x0000, 0x07ff, "Mirrored Ram #2"));
            this.MemoryMapper.Add(new MirroredMemory(0x1800, 0x1fff, 0x0000, 0x07ff, "Mirrored Ram #3"));

            // I/O Registers - $2000-$401F
            // $2000 - I/O - I/O Registers

            // this.MemoryMapper.Add(new MemoryMappedRegister());

            // $2008 - I/O - Mirrors $2000-$2007
            // $4000 - I/O - I/O Registers (DMA for sprites?)

            // Expansion ROM
            this.MemoryMapper.Add(new ROM(0x4020, 0x5FFF, "Expansion ROM"));

            // SRAM - this is mapped into a Cartridge's memory specifically .
            this.MemoryMapper.Add(new SRAM(0x6000, 0x7FFF, "SRAM"));

            // PRG-ROM - this is mapped into a cartridge, .
            this.MemoryMapper.Add(new ExternalROM(0x8000, 0xBFFF, "PRG-ROM Lower Bank"));
            this.MemoryMapper.Add(new ExternalROM(0xC000, 0xFFFF, "PRG-ROM Upper Bank"));
        }
    }
}
