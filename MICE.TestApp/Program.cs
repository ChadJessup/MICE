using MICE.Common.Interfaces;
using MICE.Components;
using MICE.CPU.LR35902;
using MICE.CPU.MOS6502;
using MICE.Nintendo;
using MICE.Nintendo.Loaders;
using MICE.PPU.RicohRP2C02;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MICE.TestApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public async static Task Main()
        {
            var nesBuilder = new SystemBuilder()
                .WithCPU<Ricoh2A03>()
                .WithLoader<NESLoader>()
                .WithMemoryMap<NESMemoryMap>()
                .WithComponent<RicohRP2C02>()
                .WithNESComponents();

            var nes = nesBuilder.Build<NES>();
            ILoader loader = nes.GetLoader();
            var cartridge = loader.Load<NESCartridge>(@"C:\Emulators\NES\Games\Super Mario Bros.nes");
            nes.Load<NESCartridge>(cartridge);

            nes.PowerOn();
            await nes.Run();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using var form = new Form1();
            Application.Run(form);
        }
    }
}
