using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MICE.WPF.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly NESViewModel nesVM;

        public MainWindowViewModel(NESViewModel nesVM)
        {
            this.nesVM = nesVM;
            this.nesVM.PropertyChanged += this.NESPropertyChanged;
        }

        private void NESPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.nesVM.IsPoweredOn):
                    this.StatusBar = this.nesVM.IsPoweredOn ? "Powered On" : "Powered Off";
                    break;
                case nameof(this.nesVM.IsPaused):
                    this.StatusBar = this.nesVM.IsPaused ? "Paused" : "Unpaused";
                    break;
                case nameof(this.nesVM.Cartridge):
                    this.statusBar = "Cartridge loaded...";
                    break;
                default:
                    break;
            }
        }

        private string statusBar = "Powered Off";
        public string StatusBar
        {
            get => this.statusBar;
            set
            {
                this.statusBar = value;
                this.NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}