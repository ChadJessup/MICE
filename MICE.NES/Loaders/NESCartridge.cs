using MICE.Common.Interfaces;
using MICE.Nintendo.Interfaces;
using MICE.Nintendo.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MICE.Nintendo.Loaders
{
    public class NESCartridge
    {
        private static class Constants
        {
            public static Dictionary<MemoryMapperIds, (Type Mapper, MapperAttribute Details)> Mappers { get; } = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.CustomAttributes.Any(ca => ca.AttributeType == typeof(MapperAttribute)))
                .Select(t => new { type = t, attr = t.GetCustomAttribute<MapperAttribute>() })
                .ToDictionary(t => t.attr.Id, t => (t.type, t.attr));
        }

        private bool WasMapperInitialized { get; set; } = false;

        public MirroringMode MirroringMode { get; set; }

        public byte[] SRAM { get; set; }

        public byte[] Trainer { get; set; }

        public string Crc32 { get; set; }

        public IList<byte[]> ProgramRAMBanks { get; set; }
        public IList<byte[]> ProgramROMBanks { get; set; }
        public IList<byte[]> CharacterRomBanks { get; set; }

        public MemoryMapperIds MapperId { get; private set; } = MemoryMapperIds.UNKNOWN;
        public IMemoryManagementController Mapper { get; set; }

        public void InitializeMapper(MemoryMapperIds memoryMapperId)
        {
            if (this.WasMapperInitialized)
            {
                throw new InvalidOperationException("Cannot initialize a cartridge's mapper twice.");
            }

            if (Constants.Mappers.TryGetValue(memoryMapperId, out var mappedTuple))
            {
                this.Mapper = (IMemoryManagementController)Activator.CreateInstance(mappedTuple.Mapper, this, memoryMapperId);
                this.WasMapperInitialized = true;

                this.MapperId = memoryMapperId;

                // TODO: Logging, log what mapper, the name, etc, was used...
            }
            else
            {
                throw new NotImplementedException($"Required Mapper ({memoryMapperId}) has not been implemented yet.");
            }
        }
    }
}
