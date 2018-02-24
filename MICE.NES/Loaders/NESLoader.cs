using MICE.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MICE.Nintendo.Loaders
{
    /// <summary>
    /// Loads and parses iNES files (*.nes).
    /// Also, provides mappable Memory segments for a MemoryMapper to map into.
    /// </summary>
    public class NESLoader : ILoader
    {
        public static NESCartridge Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Unable to find file at: {filePath}");
            }

            return NESLoader.Load(File.ReadAllBytes(filePath));
        }

        public static NESCartridge Load(byte[] bytes)
        {
            var cartridge = new NESCartridge();

            // We only suppport iNES format for now...eventually parse and handle unf;

            INESFile file = new INESFile().Parse(bytes);


            return cartridge;
        }
    }
}
