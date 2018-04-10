using MICE.Common.Interfaces;
using MICE.Nintendo.Handlers;
using MICE.PPU.RicohRP2C02;
using MICE.PPU.RicohRP2C02.Handlers;

namespace MICE.Nintendo
{
    public class NESContext
    {
        public NESContext
        (
            ICPU cpu,
            RicohRP2C02 ppu,
            InputHandler inputHandler,
            ScrollHandler scrollHandler,
            PixelMuxer pixelMuxer,
            BackgroundHandler backgroundHandler,
            PaletteHandler paletteHandler
        )
        {
            this.CPU = cpu;
            this.PPU = ppu;
            this.InputHandler = inputHandler;

            this.ScrollHandler = scrollHandler;
            this.PixelMuxer = pixelMuxer;
            this.BackgroundHandler = backgroundHandler;
            this.PaletteHandler = paletteHandler;
        }

        public ICPU CPU { get; }
        public RicohRP2C02 PPU { get; }
        public InputHandler InputHandler { get; }
        public ScrollHandler ScrollHandler { get; }
        public PixelMuxer PixelMuxer { get; }
        public BackgroundHandler BackgroundHandler { get; }
        public PaletteHandler PaletteHandler { get; }
    }
}
