using System;
using System.Windows;
using Game;
using OpenTK;
using System.Reflection;
using System.Windows.Forms;
using System.Timers;
using System.Windows.Input;
using EditorLogic;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Game.Common;
using Game.Models;
using Game.Rendering;
using Serializer = Game.Serialization.Serializer;

namespace EditorWindow
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow : System.Windows.Window, IDisposable
    {
        GlLoop _loop;
        ControllerEditor _controllerEditor;
        public static string WorkingDirectory { get; private set; }
        public static string AssetsDirectory { get; private set; }
        OpenFileDialog _loadModelDialog = new OpenFileDialog();
        ControllerFiles _controllerFiles;
        readonly Thread _wpfThread;
        static MainWindow _window;

        public MainWindow()
        {
            _window = this;
            Thread.CurrentThread.Name = "WPF Thread";
            WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AssetsDirectory = Path.Combine(WorkingDirectory, "editor assets");
            InitializeComponent();
            //Set the application window size here.  That way it can be super small in the editor!
            Width = 1920;
            Height = 1080;
            CenterWindowOnScreen();
            _loadModelDialog.FileOk += _openFileDialog_FileOk;

            _wpfThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Centers main window within display.  Doesn't work for multiple displays that are different sizes.
        /// </summary>
        /// <remarks>Code found at http://stackoverflow.com/questions/4019831/how-do-you-center-your-main-window-in-wpf </remarks>
        private void CenterWindowOnScreen()
        {
            Rect workArea = SystemParameters.WorkArea;
            Left = (workArea.Width - Width) / 2 + workArea.Left;
            Top = (workArea.Height - Height) / 2 + workArea.Top;
        }

        private void _openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _controllerEditor.AddAction(() =>
            {
                string fileName = ((OpenFileDialog)sender).FileName;
                ModelLoader loader = new ModelLoader();
                Model model = loader.LoadObj(fileName);
            });
        }

        public void GLControl_Load(object sender, EventArgs e)
        {
            _controllerEditor = new ControllerEditor(glControl.ClientSize, new Input(glControl));
            _controllerEditor.ScenePlayEvent += ControllerEditor_ScenePlayed;
            _controllerEditor.ScenePauseEvent += ControllerEditor_ScenePaused;
            _controllerEditor.SceneStopEvent += ControllerEditor_SceneStopped;
            _controllerEditor.Update += ControllerEditor_Update;

            _controllerFiles = new ControllerFiles(this, _controllerEditor, filesRecent);

            UpdateTransformLabels(null);

            PortalDepth.ValueChanged += IntegerUpDown_ValueChanged;
            PortalDepth.Value = 40;

            SetPortalRendering(true);

            ToolPanel.Initialize(_controllerEditor);
            PropertiesEditor.Initialize(_controllerEditor);
            Time.Initialize(_controllerEditor);
            
            Slider_ValueChanged(
                null, 
                new RoutedPropertyChangedEventArgs<double>(_controllerEditor.PhysicsStepSize, _controllerEditor.PhysicsStepSize)
                );

            //Start the drawing loop last to make sure all listeners are in place.
            _loop = new GlLoop(glControl, _controllerEditor);
            _loop.Run(60);
        }

        private void ControllerEditor_Update(ControllerEditor controller)
        {
            Invoke(() =>
            {
                Vector2 mousePos = _controllerEditor.GetMouseWorld();
                MouseCoordinates.Content = mousePos.X.ToString("0.00") + ", " + mousePos.Y.ToString("0.00");

                int fps = (int)Math.Round(_loop.UpdatesPerSecond * (double)_loop.MillisecondsPerStep / _loop.GetAverage());
                FrameRate.Content = "FPS " + fps.ToString() + "/" + _loop.UpdatesPerSecond.ToString();

                var selection = _controllerEditor.Selection.GetAll();
                if (selection.Count > 0)
                {
                    UpdateTransformLabels(selection[0]);
                    PropertiesEditor.SetSelected(selection[0]);
                }
                else
                {
                    UpdateTransformLabels(null);
                    PropertiesEditor.SetSelected(null);
                }
            });
        }

        public static void Invoke(Action action)
        {
            Debug.Assert(Thread.CurrentThread == _window._loop.Thread, "This method is intended to be exclusively used by the OGL Thread.");
            _window.Dispatcher.Invoke(action);
        }

        private void UpdateTransformLabels(EditorObject entity)
        {
            float angle = 0;
            Vector2 position = new Vector2();
            if (entity != null)
            {
                Transform2 transform = entity.GetWorldTransform();
                angle = MathHelper.RadiansToDegrees(transform.Rotation);
                position = transform.Position;
            }
            LabelAngle.Content = angle.ToString("0.00") + "°";
            LabelPosition.Content = "(" + position.X.ToString("0.00") + ", " + position.Y.ToString("0.00") + ")";
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            lock (_controllerEditor.ClosingLock)
            {
                _loop.Thread.Abort();
            }
            base.OnClosing(e);
            Properties.Settings.Default.Save();
        }

        private void Button_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Play(object sender, RoutedEventArgs e)
        {
            _controllerEditor.AddAction(() =>
                {
                    _controllerEditor.ScenePlay();
                });
        }

        private void Button_Pause(object sender, RoutedEventArgs e)
        {
            _controllerEditor.AddAction(() =>
                {
                    _controllerEditor.ScenePause();
                });
        }

        private void Button_Stop(object sender, RoutedEventArgs e)
        {
            _controllerEditor.AddAction(() =>
                {
                    _controllerEditor.SceneStop();
                });
        }

        private void ControllerEditor_ScenePlayed(ControllerEditor controller)
        {
            Invoke(() =>
                {
                    toolStart.IsEnabled = false;
                    toolPause.IsEnabled = true;
                    toolStop.IsEnabled = true;
                    menuRunStart.IsEnabled = false;
                    menuRunPause.IsEnabled = true;
                    menuRunStop.IsEnabled = true;

                    Status.Content = "Playing";
                });
        }

        private void ControllerEditor_ScenePaused(ControllerEditor controller)
        {
            Invoke(() =>
            {
                toolStart.IsEnabled = true;
                toolPause.IsEnabled = false;
                toolStop.IsEnabled = true;
                menuRunStart.IsEnabled = true;
                menuRunPause.IsEnabled = false;
                menuRunStop.IsEnabled = true;

                Status.Content = "Paused";
            });
        }

        private void ControllerEditor_SceneStopped(ControllerEditor controller)
        {
            Invoke(() =>
            {
                toolStart.IsEnabled = true;
                toolPause.IsEnabled = false;
                toolStop.IsEnabled = false;
                menuRunStart.IsEnabled = true;
                menuRunPause.IsEnabled = false;
                menuRunStop.IsEnabled = false;

                Status.Content = "Stopped";
            });
        }

        private void LoadModel(object sender, RoutedEventArgs e)
        {
            _loadModelDialog.Filter = "Wavefront (*.obj)|*.obj";
            _loadModelDialog.ShowDialog();
        }

        private void MainGrid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Remove arrow key responses. They interfere when trying to pan the camera in the editor.
            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                switch (e.Key)
                {
                    case Key.Left:
                    case Key.Right:
                    case Key.Up:
                    case Key.Down:
                        e.Handled = true;
                        break;
                    default:
                        break;
                }
            }
            

            if (e.Key == Key.D && 
                (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SetPortalRendering(!_controllerEditor.Renderer.PortalRenderEnabled);
            }
        }

        private void toolPortalsVisible_Click(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Primitives.ToggleButton)sender;
            bool enable = (bool)button.IsChecked;
            SetPortalRendering(enable);
        }

        /// <summary>Programatically set whether or not portals are visible.  The ui will be updated to the new value.</summary>
        private void SetPortalRendering(bool visible)
        {
            toolPortalsVisible.IsChecked = visible;
            _controllerEditor.AddAction(() =>
            {
                _controllerEditor.Renderer.PortalRenderEnabled = visible;
            });
        }

        private void Button_Save(object sender, EventArgs e)
        {
            _controllerFiles.SaveCurrent();
        }

        private void Button_Load(object sender, EventArgs e)
        {
            _controllerFiles.LoadAs();
        }

        private void Button_New(object sender, EventArgs e)
        {
            _controllerFiles.New();
        }

        private void Button_SaveAs(object sender, EventArgs e)
        {
            _controllerFiles.SaveAs();
        }

        private void Button_Undo(object sender, EventArgs e)
        {
            _controllerEditor.AddAction(() =>
            {
                _controllerEditor.Undo();
            });
        }

        private void Button_Redo(object sender, EventArgs e)
        {
            _controllerEditor.AddAction(() =>
            {
                _controllerEditor.Redo();
            });
        }

        private void CommandPlayToggle(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void CommandTimerStep(object sender, ExecutedRoutedEventArgs e)
        {
            _controllerEditor.AddAction(() =>
            {
                _controllerEditor.SceneStep();
            });
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_controllerEditor != null)
            {
                labelTimeStep.Content = e.NewValue.ToString() + "x";
                _controllerEditor.AddAction(() =>
                {
                    _controllerEditor.PhysicsStepSize = (float)e.NewValue;
                });
            }
        }

        private void Command_StepFoward(object sender, ExecutedRoutedEventArgs e)
        {
            _controllerEditor.AddAction(() => {
                _controllerEditor.SetTime(_controllerEditor.Level.Time + 1 / (double)_loop.UpdatesPerSecond);
            });
        }

        private void Command_StepBackward(object sender, ExecutedRoutedEventArgs e)
        {
            _controllerEditor.AddAction(() => {
                _controllerEditor.SetTime(_controllerEditor.Level.Time - 1 / (double)_loop.UpdatesPerSecond);
            });
        }

        private void Command_JumpFoward(object sender, ExecutedRoutedEventArgs e)
        {
            _controllerEditor.AddAction(() => {
                _controllerEditor.SetTime(_controllerEditor.Level.Time + 1);
            });
        }

        private void Command_JumpBackward(object sender, ExecutedRoutedEventArgs e)
        {
            _controllerEditor.AddAction(() => {
                _controllerEditor.SetTime(_controllerEditor.Level.Time - 1);
            });
        }

        private void Command_KeyframeAdd(object sender, ExecutedRoutedEventArgs e)
        {
            _controllerEditor.AddAction(() => {
                foreach (EditorObject instance in _controllerEditor.Selection.GetAll())
                {
                    _controllerEditor.Level.AddKeyframe(instance, instance.GetTransform());
                }
            });
        }

        private void Command_KeyframeRemove(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _controllerEditor.AddAction(() => 
            {
                _controllerEditor.Renderer.PortalRenderMax = (int)e.NewValue;
            });
        }

        private void RunStandalone(object sender, ExecutedRoutedEventArgs e)
        {
            _controllerEditor.AddAction(() => 
            {
                string tempFile;
                do {
                    tempFile = Controller.TempLevelPrefix + GenerateRandomString(8) + ".xml";
                } while (File.Exists(tempFile));

                Scene scene = LevelExport.Export(_controllerEditor.Level, _controllerEditor);
                Game.Portals.PortalCommon.UpdateWorldTransform(scene);
                Serializer serializer = new Serializer();
                serializer.Serialize(scene, tempFile);

                Process process = new Process();
                process.StartInfo.FileName = "Game.exe";
                process.StartInfo.WorkingDirectory = WorkingDirectory;
                process.StartInfo.Arguments = tempFile;
                process.Start();
            });
        }

        private string GenerateRandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random(Guid.NewGuid().GetHashCode());

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        public void Dispose()
        {
            _loadModelDialog.Dispose();
        }
    }
}
