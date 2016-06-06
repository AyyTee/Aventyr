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

namespace EditorWindow
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow : System.Windows.Window
    {
        GLLoop _loop;
        ControllerEditor ControllerEditor;
        public static string LocalDirectory { get; private set; }
        public static string AssetsDirectory { get; private set; }
        OpenFileDialog _loadModelDialog = new OpenFileDialog();
        ControllerFiles ControllerFiles;

        System.Timers.Timer updateTimer;
        public MainWindow()
        {
            Thread.CurrentThread.Name = "WPF Thread";
            LocalDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AssetsDirectory = System.IO.Path.Combine(LocalDirectory, "editor assets");
            InitializeComponent();
            //Set the application window size here.  That way it can be super small in the editor!
            Width = 1000;
            Height = 650;
            CenterWindowOnScreen();
            _loadModelDialog.FileOk += _openFileDialog_FileOk;
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
            ControllerEditor.AddAction(() =>
            {
                string fileName = ((OpenFileDialog)sender).FileName;
                ModelLoader loader = new ModelLoader();
                Model model = loader.LoadObj(fileName);
            });
        }

        public void GLControl_Load(object sender, EventArgs e)
        {
            ControllerEditor = new ControllerEditor(glControl.ClientSize, new InputExt(glControl));
            ControllerEditor.ScenePlayEvent += ControllerEditor_ScenePlayed;
            ControllerEditor.ScenePauseEvent += ControllerEditor_ScenePaused;
            ControllerEditor.SceneStopEvent += ControllerEditor_SceneStopped;
            ControllerEditor.SceneModified += ControllerEditor_SceneModified;

            ControllerFiles = new ControllerFiles(this, ControllerEditor, filesRecent);

            glControl.MouseMove += glControl_MouseMove;
            
            updateTimer = new System.Timers.Timer(500);
            updateTimer.Elapsed += new ElapsedEventHandler(UpdateFrameRate);  
            updateTimer.Enabled = true;
            UpdateTransformLabels(null);

            SetPortalRendering(true);

            ToolPanel.Initialize(ControllerEditor);
            PropertiesEditor.Initialize(ControllerEditor);
            
            Slider_ValueChanged(
                null, 
                new RoutedPropertyChangedEventArgs<double>(ControllerEditor.physicsStepSize, ControllerEditor.physicsStepSize)
                );

            //Start the drawing loop last to make sure all listeners are in place.
            _loop = new GLLoop(glControl, ControllerEditor);
            _loop.Run(60);
        }

        private void ControllerEditor_SceneModified(HashSet<EditorObject> modified)
        {
            Dispatcher.Invoke(() =>
            {
                //UpdateTransformLabels(controller.selection.First);
            });
        }

        private void UpdateFrameRate(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                int fps = (int)Math.Round(_loop.UpdatesPerSecond * (double)_loop.MillisecondsPerStep / _loop.GetAverage());
                FrameRate.Content = "FPS " + fps.ToString() + "/" + _loop.UpdatesPerSecond.ToString();
            });
        }

        private void glControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Vector2 mousePos = ControllerEditor.GetMouseWorldPosition();
            MouseCoordinates.Content = mousePos.X.ToString("0.00") + ", " + mousePos.Y.ToString("0.00");
        }

        public void ControllerEditor_EntitySelected(List<EditorObject> selection)
        {
            Dispatcher.Invoke(() =>
            {
                if (selection.Count == 1)
                {
                    UpdateTransformLabels(selection[0]);
                    PropertiesEditor.SetSelected(selection[0]);
                }
            });
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
            base.OnClosing(e);
            updateTimer.Stop();
            //Prevent this method from completing until the GL loop is finished.
            _loop.Stop();
            lock (_loop) { }

            Properties.Settings.Default.Save();
        }

        private void Button_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Play(object sender, RoutedEventArgs e)
        {
            ControllerEditor.AddAction(() =>
                {
                    ControllerEditor.ScenePlay();
                });
        }

        private void Button_Pause(object sender, RoutedEventArgs e)
        {
            ControllerEditor.AddAction(() =>
                {
                    ControllerEditor.ScenePause();
                });
        }

        private void Button_Stop(object sender, RoutedEventArgs e)
        {
            ControllerEditor.AddAction(() =>
                {
                    ControllerEditor.SceneStop();
                });
        }

        private void ControllerEditor_ScenePlayed(ControllerEditor controller)
        {
            Dispatcher.Invoke(() =>
                {
                    toolStart.IsEnabled = false;
                    toolPause.IsEnabled = true;
                    toolStop.IsEnabled = true;
                    menuRunStart.IsEnabled = false;
                    menuRunPause.IsEnabled = true;
                    menuRunStop.IsEnabled = true;
                });
        }

        private void ControllerEditor_ScenePaused(ControllerEditor controller)
        {
            Dispatcher.Invoke(() =>
            {
                toolStart.IsEnabled = true;
                toolPause.IsEnabled = false;
                toolStop.IsEnabled = true;
                menuRunStart.IsEnabled = true;
                menuRunPause.IsEnabled = false;
                menuRunStop.IsEnabled = true;
            });
        }

        private void ControllerEditor_SceneStopped(ControllerEditor controller)
        {
            Dispatcher.Invoke(() =>
            {
                toolStart.IsEnabled = true;
                toolPause.IsEnabled = false;
                toolStop.IsEnabled = false;
                menuRunStart.IsEnabled = true;
                menuRunPause.IsEnabled = false;
                menuRunStop.IsEnabled = false;
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

            if (e.Key == Key.D && 
                (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SetPortalRendering(!ControllerEditor.renderer.PortalRenderEnabled);
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
            ControllerEditor.AddAction(() =>
            {
                ControllerEditor.renderer.PortalRenderEnabled = visible;
            });
        }

        private void Button_Save(object sender, EventArgs e)
        {
            ControllerFiles.SaveCurrent();
        }

        private void Button_Load(object sender, EventArgs e)
        {
            ControllerFiles.LoadAs();
        }

        private void Button_New(object sender, EventArgs e)
        {
            ControllerFiles.New();
        }

        private void Button_SaveAs(object sender, EventArgs e)
        {
            ControllerFiles.SaveAs();
        }

        private void Button_Undo(object sender, EventArgs e)
        {
            ControllerEditor.AddAction(() =>
            {
                ControllerEditor.Undo();
            });
        }

        private void Button_Redo(object sender, EventArgs e)
        {
            ControllerEditor.AddAction(() =>
            {
                ControllerEditor.Redo();
            });
        }

        private void commandPlayToggle(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void commandTimerStep(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerEditor.AddAction(() =>
            {
                ControllerEditor.SceneStep();
            });
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControllerEditor != null)
            {
                labelTimeStep.Content = e.NewValue.ToString() + "x";
                ControllerEditor.AddAction(() =>
                {
                    ControllerEditor.physicsStepSize = (float)e.NewValue;
                });
            }
        }
    }
}
