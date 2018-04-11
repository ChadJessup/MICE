using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MICE.Nintendo.Databases
{
    public class NESDatabaseEntry
    {
        public string Name { get; set; }
        public string AltName { get; set; }
        public NESSystemType System { get; set; }
        public bool IsGoodDump { get; set; }
        public string CRC { get; set; }
        public MemoryMapperIds MapperId { get; set; }

        public int PlayerCount { get; set; }

        public int ProgramROMSize { get; set; }
        public int CharacterRomSize { get; set; }
        public int WritableRAMSize { get; set; }
        public int VideoRAMSize { get; set; }

        public bool HasBattery { get; set; }

        public RegionInfo Region { get; set; }

        public string Publisher { get; set; }
        public DateTime PublishDate { get; set; }

        public string Developer { get; set; }

        public override string ToString() => $"{this.Name} - Mapper: {this.MapperId}";
    }

    public enum NESSystemType
    {
        Famicom,
        PAL,
        PALA,
        PALB,
        NTSC,

        UNKNOWN = 99
    }
}
