using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Threading;
using Game;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;

namespace LevelEditor
{
    public partial class EditorWindow : Form
    {
        GLLoop _loop;
        public EditorWindow()
        {
            InitializeComponent();

            fileExit.Click += exitToolStripMenuItem_Click;
            FormClosing += EditorWindow_FormClosing;

            propertyGrid.SelectedObject = new Property();
        }

        private void EditorWindow_FormClosing(object sender, CancelEventArgs e)
        {
            _loop.Stop();
            lock(_loop)
            {
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _loop = new GLLoop(glControlExt);
            _loop.Run(60);
        }
    }
}