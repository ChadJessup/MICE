using Microsoft.Win32;
using System;
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
            InitializeComponent();
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

                this.viewport.LoadCartridge(openFileDialog.FileName);
            }
        }

        private void AlwaysTrue(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
    }
}
