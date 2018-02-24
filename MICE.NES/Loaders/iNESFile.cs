using System;
using System.Collections.Generic;
using System.Text;

namespace MICE.Nintendo.Loaders
{
    public class INESFile
    {
        public byte[] Header { get; private set; } // header, expecting 16 bytes;
        public string Identifier { get; private set; } // should be 'NES'
        public int Format { get; private set; } // should be '1A'
        public int ProgramROMBankCount { get; private set; } // Number of 16kb PRG-ROM Banks
        public int CharacterROMBankCount { get; private set; } // number of 8kb CHR-ROM / VROM banks, which contain pattern tables.
        public byte RomControlByte1 { get; private set; } // broken apart into flags below

        public byte RomControlByte2 { get; private set; } // broken apart into flags below.

        public int RAMBankCount { get; private set; } // number of 8KB Ram Banks, assume 1 page of RAM when this is 0.

        public byte[] Reserved { get; private set; } // reserved count, expecting 7 bytes.

        public byte[] Trainer { get; private set; } = new byte[512];

        public INESFile Parse(byte[] bytes)
        {
            this.Header = new ArraySegment<byte>(bytes, 0, 16).Array;
            this.Identifier = System.Text.Encoding.UTF8.GetString(this.Header, 0, 3).TrimEnd('\0');

            return this;
        }
    }
}
