using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Editor
{
    public static class WPFCommands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand
            (
                "Exit",
                "Exit",
                typeof(WPFCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.F4, ModifierKeys.Alt)
                }
            );

        public static readonly RoutedUICommand TimerToggle = new RoutedUICommand
            (
                "Toggle",
                "TimerToggle",
                typeof(WPFCommands),
                null
            );

        public static readonly RoutedUICommand TimerStop = new RoutedUICommand
            (
                "Stop",
                "TimerStop",
                typeof(WPFCommands),
                null
            );

        public static readonly RoutedUICommand TimerPlay = new RoutedUICommand
            (
                "Play",
                "TimerPlay",
                typeof(WPFCommands),
                null
            );

        public static readonly RoutedUICommand TimerPause = new RoutedUICommand
            (
                "Pause",
                "TimerPause",
                typeof(WPFCommands),
                null
            );
    }
}