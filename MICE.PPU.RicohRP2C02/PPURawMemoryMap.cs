using MICE.Common.Misc;
using MICE.PPU.RicohRP2C02.Components;
using System;

namespace MICE.PPU.RicohRP2C02
{
    /// <summary>
    /// A class that represents how the NES PPU's memory is mapped out.
    /// </summary>
    public class PPURawMemoryMap : MemoryMapper
    {
        // http://nesdev.com/NESDoc.pdf - Figure 3-1 PPU memory map

        private byte[] memory = new byte[0x4000];
        private Palette backgroundPalette;
        private PatternTable patternTable0;
        private PatternTable patternTable1;
        private Nametable nameTable0;
        private Nametable nameTable1;
        private Nametable nameTable2;
        private Nametable nameTable3;

        public Memory<byte> Memory { get; private set; }

        public PPURawMemoryMap() : base()
        {
            this.Memory = new Memory<byte>(this.memory);

            //// Pattern Tables
            //this.Add(new PatternTable(0x0000, 0x0FFF, "Pattern Table 0"));
            //this.Add(new PatternTable(0x1000, 0x1FFF, "Pattern Table 1"));

            //// Name Tables
            //this.Add(new Nametable(0x2000, 0x23FF, "Name Table 0"));
            //this.Add(new Nametable(0x2400, 0x27FF, "Name Table 1"));
            //this.Add(new Nametable(0x2800, 0x2BFF, "Name Table 2"));
            //this.Add(new Nametable(0x2C00, 0x2FFF, "Name Table 3"));

            //this.Add(new MirroredMemory(0X3000, 0x3EFF, 0x2000, 0x2FFF, this, "Mirrored Name Tables"));

            //// Palettes
            //this.Add(new Palette(0x3F00, 0x3F03, "Background palette 0"));
            //this.Add(new Palette(0x3F04, 0x3F07, "Background palette 1"));
            //this.Add(new Palette(0x3F08, 0x3F0B, "Background palette 2"));
            //this.Add(new Palette(0x3F0C, 0x3F0F, "Background palette 3"));

            //this.Add(new MirroredMemory(0x3F10, 0x3F10, 0x3F00, 0x3F00, this, "Mirrored Universal background color byte"));
            //this.Add(new MirroredMemory(0x3F14, 0x3F14, 0x3F04, 0x3F04, this, "Mirrored Background palette 0 byte"));
            //this.Add(new MirroredMemory(0x3F18, 0x3F18, 0x3F08, 0x3F08, this, "Mirrored Background palette 1 byte"));
            //this.Add(new MirroredMemory(0x3F1C, 0x3F1C, 0x3F0C, 0x3F0C, this, "Mirrored Background palette 2 byte"));

            //this.Add(new Palette(0x3F11, 0x3F13, "Sprite palette 0"));
            //this.Add(new Palette(0x3F15, 0x3F17, "Sprite palette 1"));
            //this.Add(new Palette(0x3F19, 0x3F1B, "Sprite palette 2"));
            //this.Add(new Palette(0x3F1D, 0x3F1F, "Sprite palette 3"));

            //this.Add(new MirroredMemory(0x3F20, 0x3FFF, 0x3F00, 0x3F1F, this, "Mirrored Palettes"));

            //// Mirrors
            //this.Add(new MirroredMemory(0x4000, 0xFFFF, 0x0000, 0x3FFF, this, "Mirrored PPU"));
        }

        /// <summary>
        /// Gets or sets the read buffer that is buffered by certain PPU Reads/Writes.
        /// </summary>
        public byte ReadBuffer { get; set; }

        public override byte ReadByte(int index)
        {
            if (index <= 0x0FFF)
            {
                return this.patternTable0.ReadByte(index);
            }
            else if (index <= 0x1FFF)
            {
                return this.patternTable1.ReadByte(index);
            }
            else if (index <= 0x23FF)
            {
                return this.nameTable0.ReadByte(index);
            }
            else if (index <= 0x27FF)
            {
                return this.nameTable1.ReadByte(index);
            }
            else if (index <= 0x2BFF)
            {
                return this.nameTable2.ReadByte(index);
            }
            else if (index <= 0x2FFF)
            {
                return this.nameTable3.ReadByte(index);
            }

            return this.Memory.Span[index];
        }

        public override ushort ReadShort(int index)
        {
            return (ushort)(this.Memory.Span[index + 1] << 8 | this.Memory.Span[index]);
        }

        public override void Write(int index, byte value)
        {
            this.Memory.Span[index] = value;
//            base.Write(index, value);
        }

        public override T GetMemorySegment<T>(string segmentName)
        {
            if (segmentName == "Background palette 0")
            {
                this.backgroundPalette = new Palette(0x3F00, 0x3F00, segmentName, new Memory<byte>(this.memory, 0x3F00, 1));
                return (T)(object)this.backgroundPalette;
            }
            else if (segmentName == "Pattern Table 0")
            {
                this.patternTable0 = new PatternTable(0x0000, 0x0FFF, "Pattern Table 0");
                return (T)(object)this.patternTable0;
            }
            else if (segmentName == "Pattern Table 1")
            {
                this.patternTable1 = new PatternTable(0x1000, 0x1FFF, "Pattern Table 1");
                return (T)(object)this.patternTable1;
            }
            else if (segmentName == "Name Table 0")
            {
                this.nameTable0 = new Nametable(0x2000, 0x23FF, "Name Table 0");
                return (T)(object)this.nameTable0;
            }
            else if (segmentName == "Name Table 1")
            {
                this.nameTable1 = new Nametable(0x2400, 0x27FF, "Name Table 1");
                return (T)(object)this.nameTable1;
            }
            else if (segmentName == "Name Table 2")
            {
                this.nameTable2 = new Nametable(0x2800, 0x2BFF, "Name Table 2");
                return (T)(object)this.nameTable2;
            }
            else if (segmentName == "Name Table 3")
            {
                this.nameTable3 = new Nametable(0x2C00, 0x2FFF, "Name Table 3");
                return (T)(object)this.nameTable3;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}