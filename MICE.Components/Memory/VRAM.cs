using System;

namespace MICE.Components.Memory
{
    public class VRAM : RAM
    {
        public VRAM(int lowerIndex, int upperIndex, string name, Memory<byte> memory)
            : base(lowerIndex, upperIndex, name, memory)
        {
        }
    }
}
