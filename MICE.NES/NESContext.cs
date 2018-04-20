using MICE.Common.Interfaces;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Handlers;
using MICE.PPU.RicohRP2C02;
using MICE.PPU.RicohRP2C02.Handlers;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace MICE.Nintendo
{
    public class NESContext
    {
        private static class Constants
        {
            public const string DebugFile = @"C:\Emulators\NES\MICE - Trace.txt";
        }

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

            if (File.Exists(Constants.DebugFile))
            {
                File.Delete(Constants.DebugFile);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File
                (
                    Constants.DebugFile,
                    outputTemplate: "{Message}{NewLine}",
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    flushToDiskInterval: TimeSpan.FromSeconds(10.0)
                )
                .Destructure.ByTransforming<Registers>(Registers.DestructureForLog)
                .CreateLogger();
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
