using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MICE.WPF.Commands
{
    public static class StaticCommands
    {
        public static readonly RoutedUICommand OpenCommand = new RoutedUICommand
        (
            text: "Open",
            name: "Open",
            ownerType: typeof(StaticCommands),
            inputGestures: new InputGestureCollection()
            {
                new KeyGesture(Key.O, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand ExitCommand = new RoutedUICommand
        (
            text: "Exit",
            name: "Exit",
            ownerType: typeof(StaticCommands),
            inputGestures: new InputGestureCollection()
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            }
        );

    }
}
