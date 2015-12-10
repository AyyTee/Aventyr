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
            Tool[] tools = new Tool[] {
                new ToolAddEntity(_controller),
                new ToolAddPortal(_controller),
                new ToolPolygon(_controller),
                new ToolPortalLinker(_controller)
            };
            BitmapImage[] bitmaps = new BitmapImage[] {
                new BitmapImage(new Uri(System.IO.Path.Combine(MainWindow.AssetsDirectory, "icons", "entityIcon.png"))),
                new BitmapImage(new Uri(System.IO.Path.Combine(MainWindow.AssetsDirectory, "icons", "portalIcon.png"))),
                new BitmapImage(new Uri(System.IO.Path.Combine(MainWindow.AssetsDirectory, "icons", "polygonIcon.png"))),
                new BitmapImage(new Uri(System.IO.Path.Combine(MainWindow.AssetsDirectory, "icons", "polygonLinkerIcon.png")))
            };
            for (int i = 0; i < tools.Length; i++)
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
