using MICE.Common.Interfaces;
using System;
using System.Threading;

namespace MICE.PPU.RicohRP2C02
{
    public class RicohRP2C02 : IMicroprocessor
    {
        public PPUMemoryMap MemoryMap { get; } = new PPUMemoryMap();

        public void PowerOn(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public int Step()
        {
            throw new NotImplementedException();
        }
    }
}
