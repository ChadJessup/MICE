using MICE.Common.Interfaces;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Handlers;
using MICE.Nintendo.Loaders;
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
            PixelMuxer pixelMuxer,
            InputHandler inputHandler,
            ScrollHandler scrollHandler,
            PaletteHandler paletteHandler,
            BackgroundHandler backgroundHandler
        )
        {
            this.CPU = cpu;
            this.PPU = ppu;
            this.PixelMuxer = pixelMuxer;
            this.InputHandler = inputHandler;
            this.ScrollHandler = scrollHandler;
            this.PaletteHandler = paletteHandler;
            this.BackgroundHandler = backgroundHandler;

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
        public PixelMuxer PixelMuxer { get; }
        public InputHandler InputHandler { get; }
        public ScrollHandler ScrollHandler { get; }
        public PaletteHandler PaletteHandler { get; }
        public BackgroundHandler BackgroundHandler { get; }
    }
}
