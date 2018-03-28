using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MICE.WPF
{
    /// <summary>
    /// Interaction logic for EmulatorViewPort.xaml
    /// </summary>
    public partial class EmulatorViewPort : UserControl
    {
        private readonly WriteableBitmap bitmap;

        public EmulatorViewPort()
        {
            this.bitmap = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Indexed8, this.GetPalette());

            InitializeComponent();
        }

        private BitmapPalette GetPalette()
        {
            var colors = new[]
            {
                Color.FromArgb(0, 84, 84, 84),
                Color.FromArgb(0, 0, 30, 116),
                Color.FromArgb(0, 8, 16, 144),
                Color.FromArgb(0, 48, 0, 136),
                Color.FromArgb(0, 68, 0, 100),
                Color.FromArgb(0, 92, 0, 48),
                Color.FromArgb(0, 84, 4, 0),
                Color.FromArgb(0, 60, 24, 0),
                Color.FromArgb(0, 32, 42, 0),
                Color.FromArgb(0, 8, 58, 0),
                Color.FromArgb(0, 0, 64, 0),
                Color.FromArgb(0, 0, 60, 0),
                Color.FromArgb(0, 0, 50, 60),
                Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(0, 152, 150, 152),
                Color.FromArgb(0, 8, 76, 196),
                Color.FromArgb(0, 48, 50, 236),
                Color.FromArgb(0, 92, 30, 228),
                Color.FromArgb(0, 136, 20, 176),
                Color.FromArgb(0, 160, 20, 100),
                Color.FromArgb(0, 152, 34, 32),
                Color.FromArgb(0, 120, 60, 0),
                Color.FromArgb(0, 84, 90, 0),
                Color.FromArgb(0, 40, 114, 0),
                Color.FromArgb(0, 8, 124, 0),
                Color.FromArgb(0, 0, 118, 40),
                Color.FromArgb(0, 0, 102, 120),
                Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(0, 236, 238, 236),
                Color.FromArgb(0, 76, 154, 236),
                Color.FromArgb(0, 120, 124, 236),
                Color.FromArgb(0, 176, 98, 236),
                Color.FromArgb(0, 228, 84, 236),
                Color.FromArgb(0, 236, 88, 180),
                Color.FromArgb(0, 236, 106, 100),
                Color.FromArgb(0, 212, 136, 32),
                Color.FromArgb(0, 160, 170, 0),
                Color.FromArgb(0, 116, 196, 0),
                Color.FromArgb(0, 76, 208, 32),
                Color.FromArgb(0, 56, 204, 108),
                Color.FromArgb(0, 56, 180, 204),
                Color.FromArgb(0, 60, 60, 60),
                Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(0, 236, 238, 236),
                Color.FromArgb(0, 168, 204, 236),
                Color.FromArgb(0, 188, 188, 236),
                Color.FromArgb(0, 212, 178, 236),
                Color.FromArgb(0, 236, 174, 236),
                Color.FromArgb(0, 236, 174, 212),
                Color.FromArgb(0, 236, 180, 176),
                Color.FromArgb(0, 228, 196, 144),
                Color.FromArgb(0, 204, 210, 120),
                Color.FromArgb(0, 180, 222, 120),
                Color.FromArgb(0, 168, 226, 144),
                Color.FromArgb(0, 152, 226, 180),
                Color.FromArgb(0, 160, 214, 228),
                Color.FromArgb(0, 160, 162, 160),
                Color.FromArgb(0, 0, 0, 0),
                Color.FromArgb(0, 0, 0, 0)
            };

            return new BitmapPalette(colors);
        }
    }
}
