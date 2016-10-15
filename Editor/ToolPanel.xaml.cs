﻿using EditorLogic;
using EditorLogic.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for ToolPanel.xaml
    /// </summary>
    public partial class ToolPanel : UserControl
    {
        Dictionary<Tool, ToolButton> ButtonMap = new Dictionary<Tool, ToolButton>();

        public ToolPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Adds tool buttons.  Intended to be called once.
        /// </summary>
        /// <param name="controller"></param>
        public void Initialize(ControllerEditor controller)
        {
            Debug.Assert(controller != null);
            controller.ToolChanged += ControllerEditor_ToolChanged;

            string AssetsDirectory = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "editor assets");
            var arguments = new[] {
                new {
                    tool = (Tool)new ToolAddEntity(controller),
                    image = new BitmapImage(new Uri(System.IO.Path.Combine(AssetsDirectory, "icons", "entityIcon.png")))
                },
                new {
                    tool = (Tool)new ToolAddPortal(controller),
                    image = new BitmapImage(new Uri(System.IO.Path.Combine(AssetsDirectory, "icons", "portalIcon.png")))
                },
                new {
                    tool = (Tool)new ToolPortalLinker(controller),
                    image = new BitmapImage(new Uri(System.IO.Path.Combine(AssetsDirectory, "icons", "polygonLinkerIcon.png")))
                },
                new {
                    tool = (Tool)new ToolAddActor(controller),
                    image = new BitmapImage(new Uri(System.IO.Path.Combine(AssetsDirectory, "icons", "entityIcon.png")))
                },
                new {
                    tool = (Tool)new ToolAddWall(controller),
                    image = new BitmapImage(new Uri(System.IO.Path.Combine(AssetsDirectory, "icons", "polygonIcon.png")))
                },
                new {
                    tool = (Tool)new ToolAddPlayer(controller),
                    image = new BitmapImage(new Uri(System.IO.Path.Combine(AssetsDirectory, "icons", "polygonIcon.png")))
                }
            };
            for (int i = 0; i < arguments.Length; i++)
            {
                AddButton(controller, arguments[i].tool, arguments[i].image);
            }
        }

        private void ControllerEditor_ToolChanged(ControllerEditor controller, Tool tool)
        {
            MainWindow.Invoke(() =>
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
            });

        }

        private void AddButton(ControllerEditor controller, Tool tool, BitmapImage buttonImage)
        {
            ToolButton button = new ToolButton(controller, tool, buttonImage);
            ToolGrid.Children.Add(button);
            ButtonMap.Add(tool, button);
        }
    }
}
