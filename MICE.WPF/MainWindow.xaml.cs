using MICE.WPF.ViewModels;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MICE.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.DataContext = new MainWindowViewModel(this.viewport.NESVM);
        }

        private void ExitCommandExecuted(object sender, ExecutedRoutedEventArgs e) => Application.Current.Shutdown();

        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.LastDirectory))
            {
                Properties.Settings.Default.LastDirectory = Environment.CurrentDirectory;
            }

            var initialDirectory = Properties.Settings.Default.LastDirectory;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "iNES files (*.nes)|*.nes|All files (*.*)|*.*",
                RestoreDirectory = true,
                InitialDirectory = initialDirectory,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Properties.Settings.Default.LastDirectory = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                Properties.Settings.Default.Save();

                (this.viewport.DataContext as NESViewModel).LoadCartridge(openFileDialog.FileName);
            }
        }

        private void HardResetCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.viewport.NESVM.PowerOff();
            this.viewport.NESVM.PowerOn();
        }

        private void PauseCommandExecuted(object sender, ExecutedRoutedEventArgs e) => this.viewport.NESVM.Pause(!this.viewport.NESVM.IsPaused);
        private void SoftResetCommandExecuted(object sender, ExecutedRoutedEventArgs e) => this.viewport.NESVM.Reset();
        private void PowerOffCommandExecuted(object sender, ExecutedRoutedEventArgs e) => this.viewport.NESVM.PowerOff();

        private void ShowMemoryViewerCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var memoryWindow = new MemoryViewerWindow(this.viewport.NESVM);

            memoryWindow.Owner = this;
            memoryWindow.Show();
        }

        private void IsSystemRunning(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = (this.viewport.DataContext as NESViewModel).IsPoweredOn;
        private void AlwaysTrue(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void IsMemoryViewerOpen(object sender, CanExecuteRoutedEventArgs e)
        {

        }
    }
}
