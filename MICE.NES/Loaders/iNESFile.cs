using System;
using System.Collections.Generic;
using System.Linq;
using MICE.Common.Helpers;

namespace MICE.Nintendo.Loaders
{
    public class INESFile
    {
        private static class Constants
        {
            public const int ROMBankSize = 16384;
            public const int ChrBankSize = 8192;
        }

        public ArraySegment<byte> Header { get; private set; } // header, expecting 16 bytes;
        public string Identifier { get; private set; } // should be 'NES'
        public int ProgramROMBankCount { get; private set; } // Number of 16kb PRG-ROM Banks
        public List<ArraySegment<byte>> ROMBanks { get; } = new List<ArraySegment<byte>>();

        public int CharacterROMBankCount { get; private set; } // number of 8kb CHR-ROM / VROM banks, which contain pattern tables.
        public List<ArraySegment<byte>> CharacterBanks { get; } = new List<ArraySegment<byte>>();

        public byte RomControlByte1 { get; private set; } // broken apart into flags below
        // bit 0     1 for vertical mirroring, 0 for horizontal mirroring.
        // bit 1     1 for battery-backed RAM at $6000-$7FFF.
        // bit 2     1 for a 512-byte trainer at $7000-$71FF.
        // bit 3     1 for a four-screen VRAM layout.
        // bit 4-7   Four lower bits of ROM Mapper Type.
        public bool IsVerticallyMirrored => this.RomControlByte1.GetBit(0);
        public bool HasBatteryBackedRAM => this.RomControlByte1.GetBit(1); // Would be located at: $6000-$7FFF
        public bool HasTrainer => this.RomControlByte1.GetBit(2); // Would be located at: $7000-$71FF
        public bool FourScreenLayout => this.RomControlByte1.GetBit(3);
        public byte MapperTypeLowerBits { get; private set; }

        public byte RomControlByte2 { get; private set; } // broken apart into flags below.
        // bit 0     1 for VS-System cartridges.
        // bit 1-3   Reserved, must be zeroes!
        // bit 4-7   Four higher bits of ROM Mapper Type.
        public bool IsVSSystemCartridge => this.RomControlByte2.GetBit(0);
        public bool Reserved0 => this.RomControlByte2.GetBit(1);
        public bool Reserved1 => this.RomControlByte2.GetBit(2);
        public bool Reserved2 => this.RomControlByte2.GetBit(3);
        public byte MapperTypeUpperBits { get; private set; }

        public int RAMBankCount { get; private set; } // number of 8KB Ram Banks, assume 1 page of RAM when this is 0.
        public List<ArraySegment<byte>> RAMBanks { get; } = new List<ArraySegment<byte>>();

        public bool IsPAL => this.Header.Get(9).GetBit(0);

        public byte[] Reserved { get; } = new byte[7]; // reserved count, expecting 7 bytes.

        public ArraySegment<byte> Trainer { get; private set; }
        public ArraySegment<byte> SRAM { get; private set; }

        public MemoryMapperTypes MemoryMappers { get; private set; }

        public INESFile Parse(byte[] bytes)
        {
            this.Header = new ArraySegment<byte>(bytes, 0, 16);
            this.Identifier = System.Text.Encoding.ASCII.GetString(this.Header.Array, 0, 4);
            this.ProgramROMBankCount = this.Header.Get(4);
            this.CharacterROMBankCount = this.Header.Get(5);
            this.RomControlByte1 = this.Header.Get(6);
            this.RomControlByte2 = this.Header.Get(7);

            this.MapperTypeLowerBits = (byte)(this.RomControlByte1 & 0xff0);
            this.MapperTypeUpperBits = (byte)(this.RomControlByte2 & 0xff0);

            this.RAMBankCount = this.Header.Get(8);

            for (int i = 0; i < 7; i++)
            {
                this.Reserved[i] = this.Header.Get(9 + i);
            }

            if (this.HasTrainer)
            {
                this.Trainer = new ArraySegment<byte>(bytes, 0x7000, 0x71ff - 0x7000);
            }

            this.MemoryMappers = (MemoryMapperTypes)(this.MapperTypeUpperBits | this.MapperTypeLowerBits >> 4);

            if (this.HasBatteryBackedRAM)
            {
                this.SRAM = new ArraySegment<byte>(bytes, 0x6000, 0x7fff - 0x6000);
            }

            for (int i = 0; i < this.ProgramROMBankCount; i++)
            {
                this.ROMBanks.Add(new ArraySegment<byte>(bytes, 16 + (i * Constants.ROMBankSize), Constants.ROMBankSize));
            }

            var romBytes = this.ROMBanks[0].ToList();

            return this;
        }
    }
}
