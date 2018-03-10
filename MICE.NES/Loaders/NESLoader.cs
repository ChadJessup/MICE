using MICE.Common.Interfaces;
using System.IO;

namespace MICE.Nintendo.Loaders
{
    /// <summary>
    /// Loads and parses iNES files (*.nes).
    /// Also, provides mappable Memory segments for a MemoryMapper to map into.
    /// </summary>
    public class NESLoader : ILoader
    {
        public static NESCartridge CreateCartridge(string filePath, StreamWriter sw)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Unable to find file at: {filePath}");
            }

            return NESLoader.CreateCartridge(File.ReadAllBytes(filePath), sw);
        }

        public static NESCartridge CreateCartridge(byte[] bytes, StreamWriter sw)
        {
            // We only suppport iNES format for now...eventually parse and handle unf and other iNES versions;
            INESFile file = new INESFile().Parse(bytes);

            var cartridge = file.LoadCartridge(sw);

            return cartridge;
        }
    }
}
