using MICE.Common;
using MICE.Common.Interfaces;
using System;

namespace MICE.CPU.LR35902
{
    public class SharpLR35902 : ICPU
    {
        private readonly IClock clock;

        public Endianness Endianness { get; } = Endianness.LittleEndian;
        public bool IsPowered { get; private set; } = false;

        public SharpLR35902(IClock clock)
        {
            this.clock = clock;
        }


        public void PowerOff()
        {
        }

        public void PowerOn(Action cycleComplete)
        {
        }

        public void Reset()
        {
        }

        public int Step()
        {
            return 0;
        }
    }
}
