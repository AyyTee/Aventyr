using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Windows.Threading;
using EditorLogic;
using Game.Common;

namespace EditorWindow
{
    /// <summary>
    /// Handles the UI side of saving and loading levels.
    /// </summary>
    public class ControllerFiles : IDisposable
    {
        SaveFileDialog _saveFileDialog;
        OpenFileDialog _loadFileDialog;
        readonly MainWindow _controllerWpf;
        readonly ControllerEditor _controllerEditor;
        public string FilepathCurrent { get; private set; }
        public bool Loading { get; private set; }
        public RecentFilesList RecentFiles;

        public ControllerFiles(MainWindow controller, ControllerEditor controllerEditor, System.Windows.Controls.MenuItem recentFiles)
        {
            _controllerWpf = controller;
            _controllerEditor = controllerEditor;
            _controllerEditor.LevelLoaded += ControllerEditor_LevelLoaded;
            _controllerEditor.LevelSaved += ControllerEditor_LevelSaved;
            _controllerEditor.LevelCreated += ControllerEditor_LevelCreated;
            _saveFileDialog = new SaveFileDialog();
            _loadFileDialog = new OpenFileDialog();
            _saveFileDialog.FileOk += _saveFileDialog_FileOk;
            _loadFileDialog.FileOk += _loadFileDialog_FileOk;
            _saveFileDialog.Filter = Serializer.FileExtensionName + " (*." + Serializer.FileExtension + ")|*." + Serializer.FileExtension + "|TXT|*.txt";
            _loadFileDialog.Filter = Serializer.FileExtensionName + " (*." + Serializer.FileExtension + ")|*." + Serializer.FileExtension + "|TXT|*.txt";
            //TODO: Figure out how to add callback for user pressing the cancel button in file dialog window.

            FilepathCurrent = null;
            Loading = false;

            RecentFiles = new RecentFilesList(recentFiles, this);
        }

        void ControllerEditor_LevelCreated(ControllerEditor controller, string filepath)
        {
            MainWindow.Invoke(() =>
            {
            });
        }

        void ControllerEditor_LevelSaved(ControllerEditor controller, string filepath)
        {
            MainWindow.Invoke(() =>
            {
                DebugEx.Assert(filepath != null);
                DebugEx.Assert(filepath != "");
                RecentFiles.AddFilepath(filepath);
                _controllerWpf.Status.Content = "Saved";
                FilepathCurrent = filepath;
            });
        }

        void ControllerEditor_LevelLoaded(ControllerEditor controller, string filepath)
        {
            MainWindow.Invoke(() =>
            {
                DebugEx.Assert(filepath != null);
                DebugEx.Assert(filepath != "");
                DebugEx.Assert(Loading);
                RecentFiles.AddFilepath(filepath);
                Loading = false;
                _controllerWpf.Status.Content = "Loaded";
                FilepathCurrent = filepath;
            });
        }

        /// <summary>
        /// Returns whether files can be saved or loaded at the moment.
        /// </summary>
        public bool IsUnlocked()
        {
            return !Loading;
        }

        public void New()
        {
            if (IsUnlocked())
            {
                FilepathCurrent = null;
                _controllerEditor.AddAction(() =>
                {
                    _controllerEditor.LevelCreate();
                });
            }
        }

        /// <summary>
        /// Save to the current filepath or prompt the user for a filepath if none exists.
        /// </summary>
        public void SaveCurrent()
        {
            if (IsUnlocked())
            {
                if (FilepathCurrent == null)
                {
                    SaveAs();
                }
                else
                {
                    Save(FilepathCurrent);
                }
            }
        }

        /// <summary>
        /// Prompt the user for a filepath and then save to it
        /// </summary>
        public void SaveAs()
        {
            if (IsUnlocked())
            {
                _controllerWpf.Status.Content = "Saving...";
                _saveFileDialog.ShowDialog();
            }
        }

        /// <summary>
        /// Prompt the user for a filepath and then load from it.
        /// </summary>
        public void LoadAs()
        {
            if (IsUnlocked())
            {
                _controllerWpf.Status.Content = "Loading...";
                _loadFileDialog.ShowDialog();
            }
        }

        void _loadFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Load(((OpenFileDialog)sender).FileName);
        }

        void _saveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Save(((SaveFileDialog)sender).FileName);
        }

        /// <summary>
        /// Save level with given filepath.
        /// </summary>
        public void Save(string filepath)
        {
            if (IsUnlocked())
            {
                _controllerEditor.AddAction(() =>
                {
                    _controllerEditor.LevelSave(filepath);
                });
            }
        }

        /// <summary>
        /// Load level with given filepath.
        /// </summary>
        public void Load(string filepath)
        {
            if (IsUnlocked())
            {
                Loading = true;
                _controllerEditor.AddAction(() =>
                {
                    //ControllerEditor.LevelNew();
                    _controllerEditor.LevelLoad(filepath);
                });
            }
        }

        public void Dispose()
        {
            _saveFileDialog.Dispose();
            _loadFileDialog.Dispose();
        }
    }
}
