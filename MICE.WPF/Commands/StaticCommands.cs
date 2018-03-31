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

        public static readonly RoutedUICommand PauseCommand = new RoutedUICommand
        (
            text: "Pause",
            name: "Pause",
            ownerType: typeof(StaticCommands),
            inputGestures: new InputGestureCollection()
            {
                new KeyGesture(Key.Escape)
            }
        );

        public static readonly RoutedUICommand SoftResetCommand = new RoutedUICommand
        (
            text: "Soft Reset",
            name: "Soft Reset",
            ownerType: typeof(StaticCommands),
            inputGestures: new InputGestureCollection()
            {
                new KeyGesture(Key.R, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand HardResetCommand = new RoutedUICommand
        (
            text: "Hard Reset",
            name: "Hard Reset",
            ownerType: typeof(StaticCommands),
            inputGestures: new InputGestureCollection()
            {
                new KeyGesture(Key.T, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand PowerOffCommand = new RoutedUICommand
        (
            text: "Power Off",
            name: "Power Off",
            ownerType: typeof(StaticCommands),
            inputGestures: new InputGestureCollection()
            {
            }
        );

        public static readonly RoutedUICommand ShowMemoryViewerCommand = new RoutedUICommand
        (
            text: "Memory Viewer",
            name: "Memory Viewer",
            ownerType: typeof(StaticCommands),
            inputGestures: new InputGestureCollection()
            {
            }
        );
    }
}
