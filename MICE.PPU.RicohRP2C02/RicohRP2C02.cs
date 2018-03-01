using MICE.Common.Interfaces;
using MICE.Components.CPU;
using System.Threading;

namespace MICE.PPU.RicohRP2C02
{
    public class RicohRP2C02 : IMicroprocessor
    {
        public PPUMemoryMap MemoryMap { get; } = new PPUMemoryMap();

        // Registers for the PPU, that the CPU has memory mapped to particular locations.
        // The memory mapping happens when the CPU is being initialized.

        /// <summary>
        /// Various bits that controls how the PPU behaves. Sometimes called PPU Control Register 1.
        /// This register is memory mapped to the CPU at $2000.
        /// </summary>
        public Register8Bit PPUCTRL = new Register8Bit("PPUCTRL");

        /// <summary>
        /// Various bits that enables masking of certain features of the PPU. Sometimes called PPU Control Register 2.
        /// This register is memory mapped to the CPU at $2001.
        /// </summary>
        public Register8Bit PPUMASK = new Register8Bit("PPUMASK");

        /// <summary>
        /// Status bits of the current state of the PPU.
        /// This register is memory mapped to the CPU at $2002.
        /// </summary>
        public Register8Bit PPUSTATUS = new Register8Bit("PPUSTATUS");

        /// <summary>
        /// The OAM read/write address.
        /// This register is memory mapped to the CPU at $2003.
        /// </summary>
        public Register8Bit OAMADDR = new Register8Bit("OAMADDR");

        /// <summary>
        /// The OAM data read/write.
        /// This register is memory mapped to the CPU at $2004.
        /// </summary>
        public Register8Bit OAMDATA = new Register8Bit("OAMDATA");

        /// <summary>
        /// Fine control of the PPU's scroll position (X, Y).
        /// This register is memory mapped to the CPU at $2005.
        /// </summary>
        public Register8Bit PPUSCROLL = new Register8Bit("PPUSCROLL");

        /// <summary>
        /// PPU read/write address.
        /// This register is memory mapped to the CPU at $2006.
        /// </summary>
        public Register8Bit PPUADDR = new Register8Bit("PPUADDR");

        /// <summary>
        /// PPU data read/write.
        /// This register is memory mapped to the CPU at $2007.
        /// </summary>
        public Register8Bit PPUDATA = new Register8Bit("PPUDATA");

        /// <summary>
        /// The OAM DMA high address.
        /// This register is memory mapped to the CPU at $4014.
        /// </summary>
        public Register8Bit OAMDMA = new Register8Bit("OAMDMA");

        public void PowerOn(CancellationToken cancellationToken)
        {
        }

        public int Step()
        {
            return 0;
        }
    }
}
