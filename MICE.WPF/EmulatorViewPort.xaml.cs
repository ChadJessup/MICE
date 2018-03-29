using MICE.Common.Helpers;
using MICE.WPF.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MICE.WPF
{
    /// <summary>
    /// Interaction logic for EmulatorViewPort.xaml
    /// </summary>
    public partial class EmulatorViewPort : UserControl
    {
        private readonly WriteableBitmap bitmap;
        private readonly DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private readonly byte[] blankScreen = new byte[256 * 240];

        public EmulatorViewPort()
        {
            this.InitializeComponent();

            this.blankScreen.Clear(0x00);

            this.DataContext = new NESViewModel();
            this.NESVM = (this.DataContext as NESViewModel);

            this.bitmap = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Indexed8, this.GetPalette());

            this.dispatcherTimer.Tick += new EventHandler(this.TimerTick);
            this.dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 60 / 1000);

            this.image.Source = this.bitmap;

            this.NESVM.PropertyChanged += this.OnNESPropertyChanged;
        }

        private void OnNESPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var nesVM = (sender as NESViewModel);

            switch (e.PropertyName)
            {
                case nameof(nesVM.IsPoweredOn):
                    if (nesVM.IsPoweredOn)
                    {
                        this.dispatcherTimer.Start();
                    }
                    else
                    {
                        this.ClearScreen();
                        this.dispatcherTimer.Stop();
                    }
                    break;
                case nameof(nesVM.IsPaused):
                    if (nesVM.IsPaused)
                    {
                        this.dispatcherTimer.Stop();
                    }
                    else
                    {
                        this.dispatcherTimer.Start();
                    }
                    break;
                default:
                    break;
            }
        }

        private void ClearScreen() => this.Draw(this.blankScreen);

        public NESViewModel NESVM { get; private set; }

        private Int32Rect bitmapRectangle = new Int32Rect(0, 0, 256, 240);
        private int stride = 256;// * (this.bitmap.Format.BitsPerPixel + 7) / 8;

        private void TimerTick(object sender, EventArgs e) => this.Draw(this.NESVM.ScreenData);

        private void Draw(byte[] screenData)
        {
            this.bitmap.Lock();
            this.bitmap.WritePixels(bitmapRectangle, screenData, this.stride, 0, 0);
            this.bitmap.Unlock();
        }

        private BitmapPalette GetPalette()
        {
            var colors = new[]
            {
                Color.FromArgb(255, 84, 84, 84),
                Color.FromArgb(255, 0, 30, 116),
                Color.FromArgb(255, 8, 16, 144),
                Color.FromArgb(255, 48, 0, 136),
                Color.FromArgb(255, 68, 0, 100),
                Color.FromArgb(255, 92, 0, 48),
                Color.FromArgb(255, 84, 4, 0),
                Color.FromArgb(255, 60, 24, 0),
                Color.FromArgb(255, 32, 42, 0),
                Color.FromArgb(255, 8, 58, 0),
                Color.FromArgb(255, 0, 64, 0),
                Color.FromArgb(255, 0, 60, 0),
                Color.FromArgb(255, 0, 50, 60),
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 152, 150, 152),
                Color.FromArgb(255, 8, 76, 196),
                Color.FromArgb(255, 48, 50, 236),
                Color.FromArgb(255, 92, 30, 228),
                Color.FromArgb(255, 136, 20, 176),
                Color.FromArgb(255, 160, 20, 100),
                Color.FromArgb(255, 152, 34, 32),
                Color.FromArgb(255, 120, 60, 0),
                Color.FromArgb(255, 84, 90, 0),
                Color.FromArgb(255, 40, 114, 0),
                Color.FromArgb(255, 8, 124, 0),
                Color.FromArgb(255, 0, 118, 40),
                Color.FromArgb(255, 0, 102, 120),
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 236, 238, 236),
                Color.FromArgb(255, 76, 154, 236),
                Color.FromArgb(255, 120, 124, 236),
                Color.FromArgb(255, 176, 98, 236),
                Color.FromArgb(255, 228, 84, 236),
                Color.FromArgb(255, 236, 88, 180),
                Color.FromArgb(255, 236, 106, 100),
                Color.FromArgb(255, 212, 136, 32),
                Color.FromArgb(255, 160, 170, 0),
                Color.FromArgb(255, 116, 196, 0),
                Color.FromArgb(255, 76, 208, 32),
                Color.FromArgb(255, 56, 204, 108),
                Color.FromArgb(255, 56, 180, 204),
                Color.FromArgb(255, 60, 60, 60),
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 236, 238, 236),
                Color.FromArgb(255, 168, 204, 236),
                Color.FromArgb(255, 188, 188, 236),
                Color.FromArgb(255, 212, 178, 236),
                Color.FromArgb(255, 236, 174, 236),
                Color.FromArgb(255, 236, 174, 212),
                Color.FromArgb(255, 236, 180, 176),
                Color.FromArgb(255, 228, 196, 144),
                Color.FromArgb(255, 204, 210, 120),
                Color.FromArgb(255, 180, 222, 120),
                Color.FromArgb(255, 168, 226, 144),
                Color.FromArgb(255, 152, 226, 180),
                Color.FromArgb(255, 160, 214, 228),
                Color.FromArgb(255, 160, 162, 160),
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 0, 0, 0)
            };

            return new BitmapPalette(colors);
        }
    }
}
