using MICE.Common.Interfaces;
using MICE.Nintendo.Handlers;
using MICE.PPU.RicohRP2C02;

namespace MICE.Nintendo
{
    public class NESContext
    {
        public NESContext
        (
            ICPU cpu,
            RicohRP2C02 ppu,
            InputHandler inputHandler
        )
        {
            this.CPU = cpu;
            this.PPU = ppu;
            this.InputHandler = inputHandler;
        }

        public ICPU CPU { get; }
        public RicohRP2C02 PPU { get; }
        public InputHandler InputHandler { get; }
    }
}
