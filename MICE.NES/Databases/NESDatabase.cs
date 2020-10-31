using ByteSizeLib;
using MICE.Nintendo.Databases.NesCartDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace MICE.Nintendo.Databases
{
    public static class NESDatabase
    {
        private static class Constants
        {
            public const string NstDatabasePath = "NstDatabase.xml";
            public const string NesCartsPath = "NesCarts.xml";
        }

        private static bool hasParsed;

        private static Dictionary<string, NESDatabaseEntry> entries = new Dictionary<string, NESDatabaseEntry>();

        public static NESDatabaseEntry GetRomDetails(string crc32)
        {
            if (!NESDatabase.hasParsed)
            {
                NESDatabase.Parse();
            }

            if (NESDatabase.entries.TryGetValue(crc32, out NESDatabaseEntry entry))
            {
                return entry;
            }

            return null;
        }

        public static void Parse()
        {
            if (File.Exists(Constants.NstDatabasePath))
            {
                NESDatabase.ParseNstDatabase(Constants.NstDatabasePath);
            }

            if (File.Exists(Constants.NesCartsPath))
            {
                NESDatabase.ParseNesCartsDatabase(Constants.NesCartsPath);
            }

            NESDatabase.hasParsed = true;

            GC.Collect(3, GCCollectionMode.Forced, blocking: true);
        }

        public static void ParseNesCartsDatabase(string nesCartsPath)
        {
            var serializer = new XmlSerializer(typeof(NesCartDB.database));

            NesCartDB.database database = (NesCartDB.database)serializer.Deserialize(new StreamReader(nesCartsPath));

            foreach (var entry in database.game)
            {
                var newEntry = new NESDatabaseEntry
                {
                    Name = entry.name,
                    AltName = entry.altname,
                    PlayerCount = entry.players,
                    Developer = entry.developer,
                    Publisher = entry.publisher,
                };

                if (DateTime.TryParse(entry.date, out DateTime date))
                {
                    newEntry.PublishDate = DateTime.Parse(entry.date);
                }

                var cartridge = entry.cartridge.First();
                newEntry.CRC = cartridge.crc;
                newEntry.IsGoodDump = cartridge.dump == "ok";

                if (Enum.TryParse(cartridge.system.Replace("-", "").Replace("NES", ""), out NESSystemType system))
                {
                    newEntry.System = system;
                }
                else
                {
                    newEntry.System = NESSystemType.UNKNOWN;
                }

                if (Enum.TryParse(cartridge.board.mapper.ToString(), out MemoryMapperIds mapperId))
                {
                    newEntry.MapperId = mapperId;
                }
                else
                {
                    newEntry.MapperId = MemoryMapperIds.UNKNOWN;
                }

                foreach (var board in cartridge.board.Items)
                {
                    switch (board)
                    {
                        case DatabaseGameCartridgeBoardPrg prg:
                            newEntry.ProgramROMSize = (int)ByteSize.FromKibiBytes(double.Parse(prg.size.Replace("k", ""))).Bytes;
                            break;
                        case DatabaseGameCartridgeBoardVram vram:
                            newEntry.VideoRAMSize = (int)ByteSize.FromKibiBytes(double.Parse(vram.size.Replace("k", ""))).Bytes;
                            break;
                        case DatabaseGameCartridgeBoardWram wram:
                            newEntry.WritableRAMSize = (int)ByteSize.FromKibiBytes(double.Parse(wram.size.Replace("k", ""))).Bytes;
                            newEntry.HasBattery = wram.battery == 1 ? true : false;
                            break;
                        case DatabaseGameCartridgeBoardChr chr:
                            newEntry.CharacterRomSize = (int)ByteSize.FromKibiBytes(double.Parse(chr.size.Replace("k", ""))).Bytes;
                            break;
                        case DatabaseGameCartridgeBoardChip chip:
                            break;
                        case DatabaseGameCartridgeBoardCic cic:
                        case DatabaseGameCartridgeBoardPad pad:
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }

                if (!NESDatabase.entries.ContainsKey(newEntry.CRC))
                {
                    NESDatabase.entries.Add(newEntry.CRC, newEntry);
                }
            }
        }

        public static void ParseNstDatabase(string nstDatabasePath)
        {
            var serializer = new XmlSerializer(typeof(NstDatabase.database));

            NstDatabase.database database = (NstDatabase.database)serializer.Deserialize(new StreamReader(nstDatabasePath));

            foreach (var entry in database.game)
            {
                var newEntry = new NESDatabaseEntry();
            }
        }
    }
}
