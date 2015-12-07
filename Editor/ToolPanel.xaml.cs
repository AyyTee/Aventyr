using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace Editor
{
    /// <summary>
    /// Interaction logic for ToolPanel.xaml
    /// </summary>
    public partial class ToolPanel : UserControl
    {
        ControllerEditor _controller;
        
        Dictionary<Tool, ToolButton> ButtonMap = new Dictionary<Tool, ToolButton>();

        public ToolPanel(ControllerEditor controller)
        {
            InitializeComponent();

            _controller = controller;
            Tool[] tools = new Tool[3];
            tools[0] = new ToolAddEntity(_controller);
            tools[1] = new ToolAddPortal(_controller);
            tools[2] = new ToolPolygon(_controller);
            BitmapImage[] bitmaps = new BitmapImage[3];
            bitmaps[0] = new BitmapImage(new Uri(System.IO.Path.Combine(MainWindow.AssetsDirectory, "icons", "entityIcon.png")));
            bitmaps[1] = new BitmapImage(new Uri(System.IO.Path.Combine(MainWindow.AssetsDirectory, "icons", "portalIcon.png")));
            bitmaps[2] = new BitmapImage(new Uri(System.IO.Path.Combine(MainWindow.AssetsDirectory, "icons", "polygonIcon.png")));
            for (int i = 0; i < 3; i++)
            {
                AddButton(tools[i], bitmaps[i]);
            }
            _controller.ToolChanged += ControllerEditor_ToolChanged;
        }

        private void ControllerEditor_ToolChanged(ControllerEditor controller, Tool tool)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (ButtonMap.ContainsKey(tool))
                {
                    ButtonMap[tool].Button.IsChecked = true;
                }
                else
                {
                    foreach (ToolButton button in ButtonMap.Values)
                    {
                        button.Button.IsChecked = false;
                    }
                }
            }));
            
        }

        private void AddButton(Tool tool, BitmapImage buttonImage)
        {
            ToolButton button = new ToolButton(_controller, tool, buttonImage);
            ToolGrid.Children.Add(button);
            ButtonMap.Add(tool, button);
        }
    }
}
