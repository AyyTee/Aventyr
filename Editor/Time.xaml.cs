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
        public Time()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the current time displayed in seconds.
        /// </summary>
        /// <param name="time"></param>
        public void SetTime(float time)
        {
            TimeValue.Text = time.ToString();
        }
    }
}
