using MICE.Common.Interfaces;
using MICE.Nintendo;
using MICE.Nintendo.Loaders;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MICE.WPF.ViewModels
{
    public class NESViewModel : INotifyPropertyChanged, ISystem
    {
        public NESViewModel()
        {
            this.NES = new NES(null);

            this.NES.StepCompleted += this.OnStepCompleted;
            this.NES.FrameFinished += this.OnFrameFinished;
            this.NES.CartridgeLoaded += this.OnCartridgeLoaded;
        }

        public NES NES { get; private set; }
        public byte[] ScreenData => this.NES.Screen;

        private NESCartridge cartridge;
        public NESCartridge Cartridge
        {
            get => this.cartridge;
            private set
            {
                this.cartridge = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool IsPoweredOn
        {
            get => this.NES?.IsPoweredOn ?? false;
            set
            {
                if (value)
                {
                    this.NES?.PowerOn();
                }
                else
                {
                    this.NES?.PowerOff();
                }

                this.NotifyPropertyChanged();
            }
        }

        public bool IsPaused
        {
            get => this.NES.IsPaused;
            set
            {
                this.NES.Pause(value);
                this.NotifyPropertyChanged();
            }
        }

        public string Name => this.NES.Name;

        public void LoadCartridge(string fileName)
        {
            this.Cartridge = NESLoader.CreateCartridge(fileName);

            this.NES.LoadCartridge(this.Cartridge);

            this.IsPoweredOn = true;

            this.NES.Run();
        }

        public void PowerOn()
        {
            this.IsPoweredOn = true;
            this.NES.Run();
        }

        public Task Run() => throw new NotImplementedException();
        public long Step() => throw new NotImplementedException();
        public void Reset() => this.NES.Reset();
        public void PowerOff()
        {
            this.IsPaused = false;
            this.IsPoweredOn = false;
        }

        public void Pause(bool isPaused = true) => this.IsPaused = isPaused;

        private void OnCartridgeLoaded(object sender, CartridgeLoadedArgs e)
        {
        }

        private void OnFrameFinished(object sender, FrameCompleteArgs e)
        {
        }

        private void OnStepCompleted(object sender, NintendoStepArgs e)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void InputChanged(object inputs)
        {
            throw new NotImplementedException();
        }
    }
}
