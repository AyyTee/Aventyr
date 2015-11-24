﻿using OpenTK.Graphics;
namespace LevelEditor
{
    partial class EditorWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorWindow));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.dropdownFile = new System.Windows.Forms.ToolStripMenuItem();
            this.fileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.fileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.fileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.fileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.eDITToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vIEWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dropdownAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.dropdownRun = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tools = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolPause = new System.Windows.Forms.ToolStripButton();
            this.toolStart = new System.Windows.Forms.ToolStripButton();
            this.toolStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.mainSplitter = new System.Windows.Forms.SplitContainer();
            this.glControlExt = new WPFControls.GLControlExt();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.menuStrip1.SuspendLayout();
            this.tools.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitter)).BeginInit();
            this.mainSplitter.Panel1.SuspendLayout();
            this.mainSplitter.Panel2.SuspendLayout();
            this.mainSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dropdownFile,
            this.eDITToolStripMenuItem,
            this.vIEWToolStripMenuItem,
            this.dropdownAdd,
            this.dropdownRun});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(937, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // dropdownFile
            // 
            this.dropdownFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileNew,
            this.fileOpen,
            this.fileSave,
            this.fileExit});
            this.dropdownFile.Name = "dropdownFile";
            this.dropdownFile.Size = new System.Drawing.Size(40, 20);
            this.dropdownFile.Text = "FILE";
            // 
            // fileNew
            // 
            this.fileNew.Image = ((System.Drawing.Image)(resources.GetObject("fileNew.Image")));
            this.fileNew.Name = "fileNew";
            this.fileNew.Size = new System.Drawing.Size(134, 22);
            this.fileNew.Text = "New";
            // 
            // fileOpen
            // 
            this.fileOpen.Image = ((System.Drawing.Image)(resources.GetObject("fileOpen.Image")));
            this.fileOpen.Name = "fileOpen";
            this.fileOpen.Size = new System.Drawing.Size(134, 22);
            this.fileOpen.Text = "Open";
            // 
            // fileSave
            // 
            this.fileSave.Image = ((System.Drawing.Image)(resources.GetObject("fileSave.Image")));
            this.fileSave.Name = "fileSave";
            this.fileSave.Size = new System.Drawing.Size(134, 22);
            this.fileSave.Text = "Save";
            // 
            // fileExit
            // 
            this.fileExit.Name = "fileExit";
            this.fileExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.fileExit.Size = new System.Drawing.Size(134, 22);
            this.fileExit.Text = "Exit";
            // 
            // eDITToolStripMenuItem
            // 
            this.eDITToolStripMenuItem.Name = "eDITToolStripMenuItem";
            this.eDITToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.eDITToolStripMenuItem.Text = "EDIT";
            // 
            // vIEWToolStripMenuItem
            // 
            this.vIEWToolStripMenuItem.Name = "vIEWToolStripMenuItem";
            this.vIEWToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.vIEWToolStripMenuItem.Text = "VIEW";
            // 
            // dropdownAdd
            // 
            this.dropdownAdd.Name = "dropdownAdd";
            this.dropdownAdd.Size = new System.Drawing.Size(43, 20);
            this.dropdownAdd.Text = "ADD";
            // 
            // dropdownRun
            // 
            this.dropdownRun.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.pauseToolStripMenuItem,
            this.stopToolStripMenuItem});
            this.dropdownRun.Name = "dropdownRun";
            this.dropdownRun.Size = new System.Drawing.Size(43, 20);
            this.dropdownRun.Text = "RUN";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("startToolStripMenuItem.Image")));
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.startToolStripMenuItem.Text = "Start";
            // 
            // pauseToolStripMenuItem
            // 
            this.pauseToolStripMenuItem.Enabled = false;
            this.pauseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pauseToolStripMenuItem.Image")));
            this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
            this.pauseToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.pauseToolStripMenuItem.Text = "Pause";
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Enabled = false;
            this.stopToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("stopToolStripMenuItem.Image")));
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            // 
            // tools
            // 
            this.tools.AutoSize = true;
            this.tools.Controls.Add(this.toolStrip1);
            this.tools.Dock = System.Windows.Forms.DockStyle.Top;
            this.tools.Location = new System.Drawing.Point(0, 24);
            this.tools.Margin = new System.Windows.Forms.Padding(0);
            this.tools.Name = "tools";
            this.tools.Size = new System.Drawing.Size(937, 25);
            this.tools.TabIndex = 8;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolPause,
            this.toolStart,
            this.toolStop,
            this.toolStripSeparator1,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(937, 25);
            this.toolStrip1.TabIndex = 8;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolPause
            // 
            this.toolPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolPause.Image = ((System.Drawing.Image)(resources.GetObject("toolPause.Image")));
            this.toolPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolPause.Name = "toolPause";
            this.toolPause.Size = new System.Drawing.Size(23, 22);
            this.toolPause.Text = "Pause";
            // 
            // toolStart
            // 
            this.toolStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStart.Image = ((System.Drawing.Image)(resources.GetObject("toolStart.Image")));
            this.toolStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStart.Name = "toolStart";
            this.toolStart.Size = new System.Drawing.Size(23, 22);
            this.toolStart.Text = "Start";
            // 
            // toolStop
            // 
            this.toolStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStop.Image = ((System.Drawing.Image)(resources.GetObject("toolStop.Image")));
            this.toolStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStop.Name = "toolStop";
            this.toolStop.Size = new System.Drawing.Size(23, 22);
            this.toolStop.Text = "Stop";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.CheckOnClick = true;
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // mainSplitter
            // 
            this.mainSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.mainSplitter.Location = new System.Drawing.Point(0, 49);
            this.mainSplitter.Name = "mainSplitter";
            // 
            // mainSplitter.Panel1
            // 
            this.mainSplitter.Panel1.Controls.Add(this.glControlExt);
            // 
            // mainSplitter.Panel2
            // 
            this.mainSplitter.Panel2.Controls.Add(this.splitContainer1);
            this.mainSplitter.Size = new System.Drawing.Size(937, 558);
            this.mainSplitter.SplitterDistance = 742;
            this.mainSplitter.TabIndex = 4;
            // 
            // glControlExt
            // 
            this.glControlExt.BackColor = System.Drawing.Color.Black;
            this.glControlExt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlExt.Location = new System.Drawing.Point(0, 0);
            this.glControlExt.Name = "glControlExt";
            this.glControlExt.Size = new System.Drawing.Size(742, 558);
            this.glControlExt.TabIndex = 2;
            this.glControlExt.VSync = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyGrid);
            this.splitContainer1.Size = new System.Drawing.Size(191, 558);
            this.splitContainer1.SplitterDistance = 293;
            this.splitContainer1.TabIndex = 5;
            // 
            // propertyGrid
            // 
            this.propertyGrid.DisabledItemForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(189, 259);
            this.propertyGrid.TabIndex = 0;
            // 
            // EditorWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(937, 607);
            this.Controls.Add(this.mainSplitter);
            this.Controls.Add(this.tools);
            this.Controls.Add(this.menuStrip1);
            this.Name = "EditorWindow";
            this.Text = "Aventyr Edityr";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tools.ResumeLayout(false);
            this.tools.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.mainSplitter.Panel1.ResumeLayout(false);
            this.mainSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitter)).EndInit();
            this.mainSplitter.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem dropdownFile;
        private System.Windows.Forms.ToolStripMenuItem fileNew;
        private System.Windows.Forms.ToolStripMenuItem fileOpen;
        private System.Windows.Forms.ToolStripMenuItem fileSave;
        private System.Windows.Forms.ToolStripMenuItem dropdownAdd;
        private System.Windows.Forms.ToolStripMenuItem fileExit;
        private System.Windows.Forms.ToolStripMenuItem dropdownRun;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.Panel tools;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolPause;
        private System.Windows.Forms.ToolStripButton toolStart;
        private System.Windows.Forms.ToolStripButton toolStop;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem eDITToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem vIEWToolStripMenuItem;
        private System.Windows.Forms.SplitContainer mainSplitter;
        private WPFControls.GLControlExt glControlExt;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PropertyGrid propertyGrid;
    }
}

