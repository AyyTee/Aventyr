using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace EditorWindow
{
    public static class WpfCommands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand
            (
                "Exit",
                "Exit",
                typeof(WpfCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.F4, ModifierKeys.Alt)
                }
            );

        public static readonly RoutedUICommand TimerToggle = new RoutedUICommand
            (
                "Toggle",
                "TimerToggle",
                typeof(WpfCommands),
                null
            );

        public static readonly RoutedUICommand TimerStop = new RoutedUICommand
            (
                "Stop",
                "TimerStop",
                typeof(WpfCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Escape)
                }
            );

        public static readonly RoutedUICommand TimerPlay = new RoutedUICommand
            (
                "Play",
                "TimerPlay",
                typeof(WpfCommands),
                null
            );

        public static readonly RoutedUICommand TimerPause = new RoutedUICommand
            (
                "Pause",
                "TimerPause",
                typeof(WpfCommands),
                null
            );

        public static readonly RoutedUICommand TimerStep = new RoutedUICommand
            (
                "Step",
                "TimerStep",
                typeof(WpfCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.P, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand TimerStepFoward = new RoutedUICommand
            (
                "Step Foward",
                "TimerStepFoward",
                typeof(WpfCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Right, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand TimerStepBackward = new RoutedUICommand
            (
                "Step Backward",
                "TimerStepBackward",
                typeof(WpfCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Left, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand TimerJumpFoward = new RoutedUICommand
            (
                "Jump Foward",
                "TimerJumpFoward",
                typeof(WpfCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Up, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand TimerJumpBackward = new RoutedUICommand
            (
                "Jump Backward",
                "TimerJumpBackward",
                typeof(WpfCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Down, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand KeyframeAdd = new RoutedUICommand
            (
                "Add Keyframe",
                "KeyframeAdd",
                typeof(WpfCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.K, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand KeyframeRemove = new RoutedUICommand
            (
                "Remove Keyframe",
                "KeyframeRemove",
                typeof(WpfCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.K, ModifierKeys.Alt)
                }
            );

        public static readonly RoutedUICommand RunStandalone = new RoutedUICommand
            (
                "Run Standalone",
                "RunStandalone",
                typeof(WpfCommands),
                null
            );
    }
}