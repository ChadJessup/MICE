using MICE.Common.Misc;
using MICE.Components.Memory;

namespace MICE.Nintendo
{
    /// <summary>
    /// A class that represents how NES memory is mapped out to various components.
    /// </summary>
    public class NESMemoryMap : MemoryMapper
    {
        public NESMemoryMap()
        {
            // NES memory is mapped out like below - with some trickery possible in the ROM itself to further map out memory.

            // http://nesdev.com/NESDoc.pdf - Figure 2-3 CPU memory map
            // RAM - $0000-$1FFF (including mirrored data)
            this.Add(new RAM(0x0000, 0x00ff, "Zero Page"));
            this.Add(new RAM(0x0100, 0x01ff, "Stack"));
            this.Add(new RAM(0x0200, 0x07ff, "RAM"));

            // $0800 - RAM - Mirrors $0000-$07FF three times.
            this.Add(new MirroredMemory(0x0800, 0x0fff, 0x0000, 0x07ff, "Mirrored Ram #1"));
            this.Add(new MirroredMemory(0x1000, 0x17FF, 0x0000, 0x07ff, "Mirrored Ram #2"));
            this.Add(new MirroredMemory(0x1800, 0x1fff, 0x0000, 0x07ff, "Mirrored Ram #3"));

            // I/O Registers - $2000-$401F
            // $2000 - I/O - I/O Registers

            // this.Add(new MemoryMappedRegister());

            // $2008 - I/O - Mirrors $2000-$2007
            // $4000 - I/O - I/O Registers (DMA for sprites?)

            // Expansion Memory - seems to be able to be used by various Mappers for various reasons
            this.Add(new ExternalROM(0x4020, 0x5FFF, "Expansion Memory"));

            // SRAM - this is mapped into a Cartridge's memory for saving data across power cycles.
            this.Add(new SRAM(0x6000, 0x7FFF, "SRAM"));

            // PRG-ROM - this is mapped into a cartridge, the data might be mirrored, across banks, or even internally routed further
            // in a cartridge depending on what, if any, Mapper is being used.
            this.Add(new External(0x8000, 0xBFFF, "PRG-ROM Lower Bank"));
            this.Add(new External(0xC000, 0xFFFF, "PRG-ROM Upper Bank"));
        }
    }
}
