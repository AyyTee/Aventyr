using System;
using System.Windows;
using Game;
using OpenTK;
using System.Reflection;
using System.Windows.Forms;
using System.Timers;
using System.Windows.Input;
using System.IO;

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        GLLoop _loop;
        ControllerEditor ControllerEditor;
        public static string LocalDirectory { get; private set; }
        public static string AssetsDirectory { get; private set; }
        OpenFileDialog _loadModelDialog = new OpenFileDialog();
        SaveFileDialog _saveFileDialog = new SaveFileDialog();
        OpenFileDialog _loadFileDialog = new OpenFileDialog();
        System.Timers.Timer updateTimer;
        public MainWindow()
        {
            LocalDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AssetsDirectory = System.IO.Path.Combine(LocalDirectory, "editor assets");
            InitializeComponent();
            //Set the application window size here.  That way it can be super small in the editor!
            Width = 1000;
            Height = 650;
            _loadModelDialog.FileOk += _openFileDialog_FileOk;
            _saveFileDialog.FileOk += _saveFileDialog_FileOk;
            _loadFileDialog.FileOk += _loadFileDialog_FileOk;
            _saveFileDialog.Filter = Serializer.fileExtensionName + " (*." + Serializer.fileExtension + ")|*." + Serializer.fileExtension;
            _loadFileDialog.Filter = Serializer.fileExtensionName + " (*." + Serializer.fileExtension + ")|*." + Serializer.fileExtension;
        }

        private void _openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ControllerEditor.AddAction(() =>
            {
                string fileName = ((OpenFileDialog)sender).FileName;
                ModelLoader loader = new ModelLoader();
                Model model = loader.LoadObj(fileName);
                EditorEntity entity = ControllerEditor.CreateLevelEntity();
                entity.Entity.AddModel(model);
            });
        }

        private void _loadFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Load(((OpenFileDialog)sender).FileName);
        }

        private void _saveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Save(((SaveFileDialog)sender).FileName);
        }

        public void GLControl_Load(object sender, EventArgs e)
        {
            ControllerEditor = new ControllerEditor(glControl.ClientSize, new InputExt(glControl));
            //ControllerEditor.EntitySelected += ControllerEditor_EntitySelected;
            ControllerEditor.ScenePlayed += ControllerEditor_ScenePlayed;
            ControllerEditor.ScenePaused += ControllerEditor_ScenePaused;
            ControllerEditor.SceneStopped += ControllerEditor_ScenePaused;
            ControllerEditor.SceneModified += ControllerEditor_SceneModified;
            glControl.MouseMove += glControl_MouseMove;
            _loop = new GLLoop(glControl, ControllerEditor);
            _loop.Run(60);

            ToolPanel ToolPanel = new ToolPanel(ControllerEditor);
            ToolGrid.Children.Add(ToolPanel);

            updateTimer = new System.Timers.Timer(500);
            updateTimer.Elapsed += new ElapsedEventHandler(UpdateFrameRate);  
            updateTimer.Enabled = true;
            UpdateTransformLabels(null);

            SetPortalRendering(true);
        }

        private void ControllerEditor_SceneModified(Editor.ControllerEditor controller, Scene scene)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                UpdateTransformLabels(controller.selection.First);
            }));
        }

        private void UpdateFrameRate(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                int fps = (int)Math.Round(_loop.UpdatesPerSecond * (double)_loop.MillisecondsPerStep / _loop.GetAverage());
                FrameRate.Content = "FPS " + fps.ToString() + "/" + _loop.UpdatesPerSecond.ToString();
            }));
        }

        private void glControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Vector2 mousePos = ControllerEditor.GetMouseWorldPosition();
            MouseWorldCoordinates.Content = mousePos.X.ToString("0.00") + ", " + mousePos.Y.ToString("0.00");
        }

        private void Save(string filename)
        {
            ControllerEditor.AddAction(() =>
                {
                    string physFilename = Path.GetFileNameWithoutExtension(filename) + "_phys" + Path.GetExtension(filename);
                    new Serializer().Serialize(ControllerEditor.Level.Root, filename, physFilename);
                });
        }

        private void Load(string filename)
        {
            ControllerEditor.AddAction(() =>
                {
                    ControllerEditor.NewLevel();
                    string physFilename = Path.GetFileNameWithoutExtension(filename) + "_phys" + Path.GetExtension(filename);
                    new Serializer().Deserialize(ControllerEditor.Level, filename, physFilename);
                });
        }

        /*private void ControllerEditor_EntitySelected(Editor.ControllerEditor controller, EditorObject entity)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                UpdateTransformLabels(controller.GetSelectedObject());
            }));
        }*/

        private void UpdateTransformLabels(EditorObject entity)
        {
            float angle = 0;
            Vector2 position = new Vector2();
            if (entity != null)
            {
                Transform2D transform = entity.GetWorldTransform();
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
            _loop.Stop();
            lock (_loop) { }
        }

        private void Button_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Play(object sender, RoutedEventArgs e)
        {
            ControllerEditor.ScenePlay();
        }

        private void Button_Pause(object sender, RoutedEventArgs e)
        {
            ControllerEditor.ScenePause();
        }

        private void Button_Stop(object sender, RoutedEventArgs e)
        {
            ControllerEditor.SceneStop();
        }

        private void ControllerEditor_ScenePaused(ControllerEditor controller, Scene scene)
        {
            toolStart.IsEnabled = true;
            toolPause.IsEnabled = false;
            toolStop.IsEnabled = false;
            menuRunStop.IsEnabled = false;
            menuRunStart.IsEnabled = true;
            menuRunPause.IsEnabled = false;
        }

        private void ControllerEditor_ScenePlayed(ControllerEditor controller, Scene scene)
        {
            toolStart.IsEnabled = false;
            toolPause.IsEnabled = true;
            toolStop.IsEnabled = true;
            menuRunStop.IsEnabled = true;
            menuRunStart.IsEnabled = false;
            menuRunPause.IsEnabled = true;
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
                case System.Windows.Input.Key.Left:
                case System.Windows.Input.Key.Right:
                case System.Windows.Input.Key.Up:
                case System.Windows.Input.Key.Down:
                    e.Handled = true;
                    break;
                default:
                    break;
            }

            if (e.Key == System.Windows.Input.Key.D && 
                (Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) ||
                Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl)))
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

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            _saveFileDialog.ShowDialog();
        }

        private void Button_Load(object sender, RoutedEventArgs e)
        {
            _loadFileDialog.ShowDialog();
        }

        private void Button_New(object sender, RoutedEventArgs e)
        {
            ControllerEditor.AddAction(() =>
            {
                ControllerEditor.NewLevel();
            });
        }
    }
}
