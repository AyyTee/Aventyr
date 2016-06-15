using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace EditorWindow
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

        public static readonly RoutedUICommand TimerStep = new RoutedUICommand
            (
                "Step",
                "TimerStep",
                typeof(WPFCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.P, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand TimerStepFoward = new RoutedUICommand
            (
                "Step Foward",
                "TimerStepFoward",
                typeof(WPFCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Right, ModifierKeys.Alt)
                }
            );
        public static readonly RoutedUICommand TimerStepBackward = new RoutedUICommand
            (
                "Step Backward",
                "TimerStepBackward",
                typeof(WPFCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Left, ModifierKeys.Alt)
                }
            );
        public static readonly RoutedUICommand TimerJumpFoward = new RoutedUICommand
            (
                "Jump Foward",
                "TimerJumpFoward",
                typeof(WPFCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Up, ModifierKeys.Alt)
                }
            );
        public static readonly RoutedUICommand TimerJumpBackward = new RoutedUICommand
            (
                "Jump Backward",
                "TimerJumpBackward",
                typeof(WPFCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Down, ModifierKeys.Alt)
                }
            );
        public static readonly RoutedUICommand KeyframeAdd = new RoutedUICommand
            (
                "Add Keyframe",
                "KeyframeAdd",
                typeof(WPFCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.K, ModifierKeys.Control)
                }
            );
        public static readonly RoutedUICommand KeyframeRemove = new RoutedUICommand
            (
                "Remove Keyframe",
                "KeyframeRemove",
                typeof(WPFCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.K, ModifierKeys.Alt)
                }
            );
    }
}