using ByteSizeLib;
using ICSharpCode.SharpZipLib.Checksum;
using MICE.Common.Helpers;
using MICE.Nintendo.Databases;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MICE.Nintendo.Loaders
{
    public class INESFile
    {
        private static class Constants
        {
            public static int ROMBankSize = (int)ByteSize.FromKiloBytes(16.0).Bytes;
            public const int ChrBankSize = 8192;

            public const int SizeOfHeader = 16;

            public const uint FileSignature = 0x1A53454E;

            public const int SizeOfSRAM = 0x7FFF - 0x6000;
            public const int LocationOfSRAM = 0x6000;

            public const int SizeOfTrainer = 0x71ff - 0x7000;
            public const int LocationOfTrainer = 0x7000;
        }

        public int INESFileVersion { get; private set; } = 1;

        public ArraySegment<byte> Header { get; private set; }

        public int ProgramROMBankCount { get; private set; } // Number of 16kb PRG-ROM Banks
        public List<byte[]> ROMBanks { get; } = new List<byte[]>();

        public int CharacterROMBankCount { get; private set; } // number of 8kb CHR-ROM / VROM banks, which contain pattern tables.
        public List<byte[]> CharacterBanks { get; } = new List<byte[]>();

        public int RAMBankCount { get; private set; } // number of 8KB Ram Banks, assume 1 page of RAM when this is 0.
        public List<byte[]> RAMBanks { get; } = new List<byte[]>();

        public byte[] Trainer { get; private set; }
        public byte[] SRAM { get; private set; }

        public byte RomControlByte1 { get; private set; } // broken apart into flags below
        // bit 0     1 for vertical mirroring, 0 for horizontal mirroring.
        // bit 1     1 for battery-backed RAM at $6000-$7FFF.
        // bit 2     1 for a 512-byte trainer at $7000-$71FF.
        // bit 3     1 for a four-screen VRAM layout.
        // bit 4-7   Four lower bits of ROM Mapper Type.
        public bool IsVerticallyMirrored => this.RomControlByte1.GetBit(0) && this.RomControlByte1.GetBit(3) == false;
        public bool HasBatteryBackedRAM => this.RomControlByte1.GetBit(1); // Would be located at: $6000-$7FFF
        public bool HasTrainer => this.RomControlByte1.GetBit(2); // Would be located at: $7000-$71FF
        public bool FourScreenLayout => this.RomControlByte1.GetBit(3);
        public byte MapperTypeLowerBits { get; private set; }

        public byte RomControlByte2 { get; private set; } // broken apart into flags below.
        // bit 0     1 for VS-System cartridges.
        // bit 1-3   Reserved, must be zeroes.
        // bit 4-7   Four higher bits of ROM Mapper Type.
        public bool IsVSSystemCartridge => this.RomControlByte2.GetBit(0);
        public bool Reserved1 => this.RomControlByte2.GetBit(1);
        public bool Reserved2 => this.RomControlByte2.GetBit(2);
        public bool Reserved3 => this.RomControlByte2.GetBit(3);
        public byte MapperTypeUpperBits { get; private set; }

        public bool IsPAL => this.Header.Get(9).GetBit(0);

        public byte[] Reserved { get; } = new byte[7]; // reserved count, expecting 7 bytes.

        public MemoryMapperIds MemoryMapperId { get; private set; }
        public MirroringMode MirroringMode { get; private set; }

        public INESFile Parse(byte[] bytes)
        {
            this.Header = new ArraySegment<byte>(bytes, 0, 16);

            // Expecting NES<eof> at the start of the file.
            var fileIdentifier = BitConverter.ToUInt32(this.Header.Array, 0);
            if (fileIdentifier != Constants.FileSignature)
            {
                throw new InvalidOperationException("File does not have expected header for iNES file.");
            }

            this.ProgramROMBankCount = this.Header.Get(4);
            this.CharacterROMBankCount = this.Header.Get(5);
            this.RAMBankCount = this.Header.Get(8);

            if (this.RAMBankCount == 0)
            {
                this.RAMBankCount = 1;
            }

            this.RomControlByte1 = this.Header.Get(6);
            this.RomControlByte2 = this.Header.Get(7);

            this.MapperTypeLowerBits = (byte)(this.RomControlByte1 & 0xff0);
            this.MapperTypeUpperBits = (byte)(this.RomControlByte2 & 0xff0);

            // We pull the memory mapper id from the two separate bytes above and combine them here...
            this.MemoryMapperId = (MemoryMapperIds)(this.MapperTypeUpperBits | this.MapperTypeLowerBits >> 4);

            this.MirroringMode = this.RomControlByte1.GetBit(0)
                ? MirroringMode.Vertical
                : MirroringMode.Horizontal;

            // The mirroring mode can be overridden a couple bits later...
            this.MirroringMode = this.RomControlByte1.GetBit(3)
                ? MirroringMode.FourScreen
                : this.MirroringMode;

            for (int i = 0; i < 7; i++)
            {
                this.Reserved[i] = this.Header.Get(9 + i);
            }

            if (this.HasTrainer)
            {
                this.Trainer = new ArraySegment<byte>(bytes, Constants.LocationOfTrainer, Constants.SizeOfTrainer).ToArray();
            }

            if (this.HasBatteryBackedRAM)
            {
                // TODO: We'll want to load in a file here at a later time
                this.SRAM = new ArraySegment<byte>(bytes, Constants.LocationOfSRAM, Constants.SizeOfSRAM).ToArray();
            }
            else
            {
                this.SRAM = new byte[Constants.SizeOfSRAM];
            }

            int romBankOffset = this.HasTrainer
                ? Constants.SizeOfHeader + Constants.SizeOfTrainer
                : Constants.SizeOfHeader;

            for (int i = 0; i < this.ProgramROMBankCount; i++)
            {
                this.ROMBanks.Add(new ArraySegment<byte>(bytes, romBankOffset + (i * Constants.ROMBankSize), Constants.ROMBankSize).ToArray());
            }

            int chrBankOffset = romBankOffset + this.ROMBanks.Sum(bank => bank.Length);

            for (int i = 0; i < this.CharacterROMBankCount; i++)
            {
                this.CharacterBanks.Add(new ArraySegment<byte>(bytes, chrBankOffset + (i * Constants.ChrBankSize), Constants.ChrBankSize).ToArray());
            }

            return this;
        }

        public NESCartridge LoadCartridge()
        {
            var cartridge = new NESCartridge
            {
                CharacterRomBanks = this.CharacterBanks,
                MirroringMode = this.MirroringMode,
                ProgramRAMBanks = this.RAMBanks,
                ProgramROMBanks = this.ROMBanks,
                Trainer = this.Trainer,
                SRAM = this.SRAM,
            };

            var crc = new Crc32();

            for (int i = 0; i < cartridge.ProgramROMBanks.Count; i++)
            {
                crc.Update(cartridge.ProgramROMBanks[i]);
            }

            for (int i = 0; i < cartridge.CharacterRomBanks.Count; i++)
            {
                crc.Update(cartridge.CharacterRomBanks[i]);
            }

            cartridge.Crc32 = crc.Value.ToString("X");

            var entry = NESDatabase.GetRomDetails(cartridge.Crc32);

            if (entry != null && entry.MapperId != this.MemoryMapperId)
            {
                Log.Information($"Mapper in the ROM ({this.MemoryMapperId}) is not what the databases say...({entry.MapperId}) trusting databases.");
                this.MemoryMapperId = entry.MapperId;
            }

            cartridge.InitializeMapper(this.MemoryMapperId);

            return cartridge;
        }
    }
}
