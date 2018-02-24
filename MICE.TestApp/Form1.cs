using MICE.Nintendo;
using MICE.Nintendo.Loaders;
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
            var cartridge = NESLoader.Load(@"C:\Emulators\NES\Games\Super Mario Bros.nes");
//            var cartridge = NESLoader.Load(@"G:\Emulators\NES\Games\Super Mario Bros.nes");
            var nes = new NES();
        }
    }
}
