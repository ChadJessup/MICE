using System;
using System.IO;
using MICE.Common.Misc;
using MICE.Components.Memory;
using MICE.CPU.MOS6502;

namespace MICE.Nintendo
{
    /// <summary>
    /// A class that represents how the NES CPU's memory is mapped out to various components.
    /// </summary>
    public class CPUMemoryMap : MemoryMapper
    {
        public CPUMemoryMap(PPU.RicohRP2C02.PPURegisters ppuRegisters, StreamWriter sw) : base(sw)
        {
            // The NES CPU's memory is mapped out like below - with some trickery possible in the ROM itself to further map out memory.

            // http://nesdev.com/NESDoc.pdf - Figure 2-3 CPU memory map
            // RAM - $0000-$1FFF (including mirrored data)
            this.Add(new RAM(0x0000, 0x00ff, "Zero Page"));
            this.Add(new Stack(0x0100, 0x01ff, "Stack", sw));
            this.Add(new RAM(0x0200, 0x07ff, "RAM"));

            // $0800 - RAM - Mirrors $0000-$07FF three times.
            this.Add(new MirroredMemory(0x0800, 0x0fff, 0x0000, 0x07ff, this, "Mirrored Ram #1"));
            this.Add(new MirroredMemory(0x1000, 0x17FF, 0x0000, 0x07ff, this, "Mirrored Ram #2"));
            this.Add(new MirroredMemory(0x1800, 0x1fff, 0x0000, 0x07ff, this, "Mirrored Ram #3"));

            // I/O Registers - $2000-$401F
            // These memory locations are mapped to the PPU's registers.
            this.Add(new MemoryMappedRegister<byte>(0x2000, 0x2000, ppuRegisters.PPUCTRL, "Mapped PPU Control Register 1"));
            this.Add(new MemoryMappedRegister<byte>(0x2001, 0x2001, ppuRegisters.PPUMASK, "Mapped PPU Control Register 2"));
            this.Add(new MemoryMappedRegister<byte>(0x2002, 0x2002, ppuRegisters.PPUSTATUS, "Mapped PPU Status Register"));
            this.Add(new MemoryMappedRegister<byte>(0x2003, 0x2003, ppuRegisters.OAMADDR, "Mapped PPU SPR-RAM Address Register"));
            this.Add(new MemoryMappedRegister<byte>(0x2004, 0x2004, ppuRegisters.OAMDATA, "Mapped PPU SPR-RAM I/O Register"));
            this.Add(new MemoryMappedRegister<byte>(0x2005, 0x2005, ppuRegisters.PPUSCROLL, "Mapped PPU Scroll Position Register"));
            this.Add(new MemoryMappedRegister<byte>(0x2006, 0x2006, ppuRegisters.PPUADDR, "Mapped PPU Read/Write Address Register"));
            this.Add(new MemoryMappedRegister<byte>(0x2007, 0x2007, ppuRegisters.PPUDATA, "Mapped PPU Read/Write Data Register"));

            // $2008 - I/O - Mirrors $2000-$2007 in a repeating pattern until $3FFF.
            this.Add(new MirroredMemory(0x2008, 0x3FFF, 0x2000, 0x2007, this, "Mirrored PPU Registers"));

            // TODO: 4000 - 4017 = APU...not implement now, but we need to map it still.
            this.Add(new RAM(0x4000, 0x4003, "APU Pulse 1 Channel"));
            this.Add(new RAM(0x4004, 0x4007, "APU Pulse 2 Channel"));

            this.Add(new RAM(0x4008, 0x400B, "APU Triangle Channel"));

            this.Add(new RAM(0x400C, 0x400F, "APU Noise Channel"));

            this.Add(new RAM(0x4010, 0x4013, "APU DMC Channel"));
            this.Add(new RAM(0x4015, 0x4015, "APU Channel Status"));
            this.Add(new RAM(0x4017, 0x4017, "APU Frame Counter"));

            // TODO: 4010 - 4013 = APU...not implemented now, but we need to map it still.
            //this.Add(new RAM(0x4011, 0x4011, "APU DMA Load Counter"));

            // $4014 - I/O - I/O Registers (DMA for sprites)
            this.Add(new MemoryMappedRegister<byte>(0x4014, 0x4014, ppuRegisters.OAMDMA, "Mapped PPU Sprite DMA Register"));

            // TODO: 4016 Input...not implemented now, but we need to map it still.
            this.Add(new RAM(0x4016, 0x4016, "Control Input 1"));

            // TODO: 4017 Input...not implemented now, but we need to map it still.
            // Duplicated with APU register.
            // this.Add(new RAM(0x4017, 0x4017, "Control Input 2"));

            // Expansion Memory - seems to be able to be used by various Mappers for various reasons
            // TODO: Route into cartridge as well?
            this.Add(new ExternalROM(0x4020, 0x5FFF, "Expansion Memory"));

            // SRAM - this is mapped into a Cartridge's memory for saving data across power cycles.
            this.Add(new SRAM(0x6000, 0x7FFF, "SRAM"));

            // PRG-ROM - this is mapped into a cartridge, the data might be mirrored across banks, or even internally routed further
            // in a cartridge depending on what, if any, Mapper is being used.
            // However, we assume there is always a Mapper which is essentially the noop (NROM) Mapper.
            this.Add(new External(0x8000, 0xBFFF, "PRG-ROM Lower Bank"));
            this.Add(new External(0xC000, 0xFFFF, "PRG-ROM Upper Bank"));
        }
    }
}
