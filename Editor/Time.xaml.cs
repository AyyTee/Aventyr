using EditorLogic;
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

namespace EditorWindow
{
    /// <summary>
    /// Interaction logic for Time.xaml
    /// </summary>
    public partial class Time : UserControl
    {
        ControllerEditor _controllerEditor;
        double _time;

        public Time()
        {
            InitializeComponent();
        }

        public void Initialize(ControllerEditor controllerEditor)
        {
            _controllerEditor = controllerEditor;
            _controllerEditor.TimeChanged += Update;
            _controllerEditor.ScenePlayEvent += _controllerEditor_ScenePlayEvent;
            _controllerEditor.SceneStopEvent += _controllerEditor_SceneStopEvent;
        }

        void _controllerEditor_SceneStopEvent(ControllerEditor controller)
        {
            MainWindow.Invoke(() => {
                TimeValue.IsEnabled = true;
                _update(controller.GetTime());
            });
        }

        void _controllerEditor_ScenePlayEvent(ControllerEditor controller)
        {
            MainWindow.Invoke(() => {
                TimeValue.IsEnabled = false;
                _update(controller.GetTime());
            });
        }

        void TimeValue_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            double result;
            if (double.TryParse(TimeValue.Text, out result) && result >= 0)
            {
                SetTime(result);
            }
            else
            {
                TimeValue.Text = _time.ToString("0.00");
            }
        }

        void Update(ControllerEditor controllerEditor, double time)
        {
            MainWindow.Invoke(() => {
                _update(time);
            });
        }

        void _update(double time)
        {
            _time = time;
            TimeValue.Text = _time.ToString("0.00");
        }

        /// <summary>
        /// Set the current time displayed in seconds.
        /// </summary>
        public void SetTime(double time)
        {
            if (_time != time)
            {
                _time = Math.Max(0, time);
                TimeValue.Text = _time.ToString("0.00");
                _controllerEditor.AddAction(() => {
                    _controllerEditor.SetTime(_time);
                });
            }
        }

        void TimeValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
            }
        }
    }
}
