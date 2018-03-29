using MICE.Nintendo.Loaders;
using System;

namespace MICE.Nintendo
{
    public class CartridgeLoadedArgs : EventArgs
    {
        public CartridgeLoadedArgs(NESCartridge cartridge)
        {
            this.Cartridge = cartridge;
        }

        public NESCartridge Cartridge { get; }
    }
}