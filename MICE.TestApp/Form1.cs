using MICE.Nintendo;
using MICE.Nintendo.Loaders;
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
            var nes = new NES();
            var cartridge = NESLoader.CreateCartridge(@"C:\Emulators\NES\Games\Super Mario Bros.nes");
           // var cartridge = NESLoader.CreateCartridge(@"G:\Emulators\NES\Games\Super Mario Bros.nes");

            nes.LoadCartridge(cartridge);
            Task.Factory.StartNew(() => nes.PowerOn());
        }
    }
}
