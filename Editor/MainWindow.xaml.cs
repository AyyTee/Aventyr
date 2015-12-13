using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Threading;
using Game;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;
using System.IO;
using System.Reflection;
using WPFControls;
using System.Windows.Forms;

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        GLLoop _loop;
        ControllerEditor ControllerEditor;
        //public Entity SelectedEntity { get; private set; }
        delegate void SetControllerCallback(Entity entity);
        public static string LocalDirectory { get; private set; }
        public static string AssetsDirectory { get; private set; }
        OpenFileDialog _openFileDialog = new OpenFileDialog();

        public MainWindow()
        {
            LocalDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AssetsDirectory = System.IO.Path.Combine(LocalDirectory, "editor assets");
            InitializeComponent();

            _openFileDialog.FileOk += _openFileDialog_FileOk;
        }

        public void GLControl_Load(object sender, EventArgs e)
        {
            ControllerEditor = new ControllerEditor(glControl.ClientSize, new InputExt(glControl));
            ControllerEditor.EntitySelected += ControllerEditor_EntitySelected;
            ControllerEditor.ScenePlayed += ControllerEditor_ScenePlayed;
            ControllerEditor.ScenePaused += ControllerEditor_ScenePaused;
            ControllerEditor.SceneStopped += ControllerEditor_ScenePaused;
            glControl.MouseMove += glControl_MouseMove;
            _loop = new GLLoop(glControl, ControllerEditor);
            _loop.Run(60);

            ToolPanel ToolPanel = new ToolPanel(ControllerEditor);
            ToolGrid.Children.Add(ToolPanel);
        }

        private void glControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Vector2 mousePos = ControllerEditor.GetMouseWorldPosition();

            MouseWorldCoordinates.Content = mousePos.X.ToString("0.00") + ", " + mousePos.Y.ToString("0.00");
        }

        private void ControllerEditor_EntitySelected(Editor.ControllerEditor controller, EditorObject entity)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
            }));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
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
            _openFileDialog.Filter = "Wavefront (*.obj)|*.obj";
            _openFileDialog.ShowDialog();
        }

        private void _openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ControllerEditor.AddAction(() =>
            {
                string fileName = ((OpenFileDialog)sender).FileName;
                ModelLoader loader = new ModelLoader();
                Model model = loader.LoadObj(fileName);
                EditorEntity entity = ControllerEditor.CreateLevelEntity();
                entity.Entity.Models.Add(model);
            });
        }
    }
}
