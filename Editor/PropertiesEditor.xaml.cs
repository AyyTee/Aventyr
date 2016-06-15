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
using EditorLogic;
using EditorLogic.Command;
using System.Diagnostics;

namespace EditorWindow
{
    /// <summary>
    /// Interaction logic for PropertiesEditor.xaml
    /// </summary>
    public partial class PropertiesEditor : UserControl
    {
        EditorObject _selected;
        ControllerEditor _controller;

        public PropertiesEditor()
        {
            InitializeComponent();
        }

        public void Initialize(ControllerEditor controller)
        {
            _controller = controller;
            _controller.SceneModified += _controller_SceneModified;
            _controller.LevelChanged += (ControllerEditor control, string filepath) =>
            {
                control.selection.SelectionChanged += Selection_SelectionChanged;
            };
            SetSelected(null);
        }

        private void Selection_SelectionChanged(List<EditorObject> selection)
        {
            MainWindow.Invoke(() =>
            {
                if (selection.Count == 1)
                {
                    SetSelected(selection[0]);
                }
                else if (selection.Count == 0)
                {
                    SetSelected(null);
                }
            });
        }

        private void _controller_SceneModified(HashSet<EditorObject> modified)
        {
            MainWindow.Invoke(() =>
            {
                EditorObject selected = modified.FirstOrDefault(item => item == _selected);
                if (selected != null)
                {
                    ObjectName.Text = selected.Name;
                }
            });
        }

        public void SetSelected(EditorObject selected)
        {
            _selected = selected;
            if (_selected == null)
            {
                IsEnabled = false;
                Type.Content = "";
                ObjectName.Text = "";
            }
            else
            {
                Type.Content = selected.GetType().ToString();
                ObjectName.Text = selected.Name;
                IsEnabled = true;
            }
            
        }

        private void ObjectName_LostFocus(object sender, RoutedEventArgs e)
        {
            Debug.Assert(_selected != null);
            //Make a copy of the textbox text so we don't try accessing it from the OGL thread.
            string text = ObjectName.Text;
            _controller.AddAction(() => { _controller.StateList.Add(new Rename(_selected, text), true); });
        }

        private void ObjectName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
            }
        }
    }
}
