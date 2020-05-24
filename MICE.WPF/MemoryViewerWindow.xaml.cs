using MICE.Common.Interfaces;
using MICE.WPF.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MICE.WPF
{
    /// <summary>
    /// Interaction logic for MemoryViewerWindow.xaml
    /// </summary>
    public partial class MemoryViewerWindow : Window
    {
        private static class Constants
        {
        }

        private readonly MemoryViewerViewModel VM;

        public MemoryViewerWindow(NESViewModel nesVM)
        {
            this.VM = new MemoryViewerViewModel(nesVM);
            this.DataContext = this.VM;

            //this.InitializeComponent();
        }

        private void MemorySelectorLoaded(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;

            var memorySegments = this.VM.GetCPUMemorySegments().ToList();
            memorySegments.AddRange(this.VM.GetPPUMemorySegments());

            comboBox.ItemsSource = memorySegments;
            comboBox.DisplayMemberPath = "Name";
            comboBox.SelectedIndex = 0;
        }

        private void MemorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (sender as ComboBox);
            var memorySegment = (IMemorySegment)comboBox.SelectedItem;

            //this.HexEditor.DataContext = memorySegment.GetBytes();
//            var memorySection = this.VM.GetMemorySegment(selectedItem.Key);
        }
    }
}
