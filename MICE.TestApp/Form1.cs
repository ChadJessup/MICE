using MICE.Nintendo;
using MICE.Nintendo.Loaders;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MICE.TestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Go();
        }

        private void Go()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var nes = new NES(token);
            var cartridge = NESLoader.CreateCartridge(@"G:\Emulators\NES\Games\World\Donkey Kong (JU).nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\Emulators\NES\Games\Super Mario Bros.nes");
            //var cartridge = NESLoader.CreateCartridge(@"G:\Emulators\NES\Games\Super Mario Bros.nes");

            nes.LoadCartridge(cartridge);
            nes.PowerOn();

            Task.Factory.StartNew(() => nes.Run());
        }
    }
}
