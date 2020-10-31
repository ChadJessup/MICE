using MICE.Common.Interfaces;
using MICE.Components;
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
        public static void Main()
        {
            var nesBuilder = new SystemBuilder()
                .WithCPU<Ricoh2A03>()
                .WithLoader<NESLoader>()
                .WithMemoryMap<NESRawMemoryMap>()
                .WithComponent<RicohRP2C02>()
                .WithNESComponents();

            var nes = nesBuilder.Build<NES>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using var form = new Form1(nes);
            Application.Run(form);
        }
    }
}
