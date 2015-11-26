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
        ControllerEditor ControllerEditor;
        delegate void SetControllerCallback(Entity entity);
        TrackBar toolTrackTime;

        public EditorWindow()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();

            SuspendLayout();
            toolTrackTime = new TrackBar();
            toolTrackTime.AutoSize = false;
            toolTrackTime.Height = 23;
            toolTrackTime.TickStyle = TickStyle.None;
            //toolTrackTime.BackColor = (Color)new Color4(0, 0, 0, 0);
            int index = toolStrip.Items.IndexOf(toolLabelTime) + 1;
            toolStrip.Items.Insert(index, new ToolStripControlHost(toolTrackTime));
            //t.ValueChanged += 
            ResumeLayout(true);

            fileExit.Click += exitToolStripMenuItem_Click;
            FormClosing += EditorWindow_FormClosing;

            toolStart.Click += new System.EventHandler(toolStart_Click);
            toolPause.Click += new System.EventHandler(toolPause_Click);
            toolStop.Click += new System.EventHandler(toolStop_Click);
            runStart.Click += new System.EventHandler(toolStart_Click);
            runPause.Click += new System.EventHandler(toolPause_Click);
            runStop.Click += new System.EventHandler(toolStop_Click);
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
            ControllerEditor = new ControllerEditor(glControlExt.ClientSize, new InputExt(glControlExt));
            ControllerEditor.EntityAdded += ControllerEditor_EntityAdded;
            ControllerEditor.ScenePlayed += ControllerEditor_ScenePlayed;
            ControllerEditor.ScenePaused += ControllerEditor_ScenePaused;
            ControllerEditor.SceneStopped += ControllerEditor_ScenePaused;
            _loop = new GLLoop(glControlExt, ControllerEditor);
            _loop.Run(60);
        }

        private void ControllerEditor_ScenePaused(LevelEditor.ControllerEditor controller, Scene scene)
        {
            propertyGrid.Enabled = true;
            toolStart.Enabled = true;
            toolPause.Enabled = false;
            toolStop.Enabled = false;
            runStop.Enabled = false;
            runStart.Enabled = true;
            runPause.Enabled = false;
        }

        private void ControllerEditor_ScenePlayed(LevelEditor.ControllerEditor controller, Scene scene)
        {
            propertyGrid.Enabled = false;
            toolStart.Enabled = false;
            toolPause.Enabled = true;
            toolStop.Enabled = true;
            runStop.Enabled = true;
            runStart.Enabled = false;
            runPause.Enabled = true;
        }

        private void ControllerEditor_EntityAdded(LevelEditor.ControllerEditor controller, Entity entity)
        {
            SetCurrentEntity(entity);
        }

        private void SetCurrentEntity(Entity entity)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.propertyGrid.InvokeRequired)
            {
                SetControllerCallback d = new SetControllerCallback(SetCurrentEntity);
                this.Invoke(d, new object[] { entity });
            }
            else
            {
                propertyGrid.SelectedObject = new EntityProperty(entity);
            }
        }

        private void toolStart_Click(object sender, EventArgs e)
        {
            ControllerEditor.ScenePlay();
        }

        private void toolPause_Click(object sender, EventArgs e)
        {
            ControllerEditor.ScenePause();
        }

        private void toolStop_Click(object sender, EventArgs e)
        {
            ControllerEditor.SceneStop();
        }
    }
}