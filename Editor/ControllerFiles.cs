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

namespace Editor
{
    /// <summary>
    /// Handles the UI side of saving and loading levels.  This class should be exclusively used by the WPF thread.  
    /// Race conditions will likely arise if this class is used by the GL thread.
    /// </summary>
    public class ControllerFiles
    {
        SaveFileDialog _saveFileDialog;
        OpenFileDialog _loadFileDialog;
        readonly MainWindow ControllerWPF;
        readonly ControllerEditor ControllerEditor;
        public string FilepathCurrent { get; private set; }
        public bool Loading { get; private set; }
        public RecentFilesList RecentFiles;

        public ControllerFiles(MainWindow controller, ControllerEditor controllerEditor, System.Windows.Controls.MenuItem recentFiles)
        {
            ControllerWPF = controller;
            ControllerEditor = controllerEditor;
            ControllerEditor.LevelLoaded += ControllerEditor_LevelLoaded;
            ControllerEditor.LevelSaved += ControllerEditor_LevelSaved;
            _saveFileDialog = new SaveFileDialog();
            _loadFileDialog = new OpenFileDialog();
            _saveFileDialog.FileOk += _saveFileDialog_FileOk;
            _loadFileDialog.FileOk += _loadFileDialog_FileOk;
            _saveFileDialog.Filter = Serializer.fileExtensionName + " (*." + Serializer.fileExtension + ")|*." + Serializer.fileExtension;
            _loadFileDialog.Filter = Serializer.fileExtensionName + " (*." + Serializer.fileExtension + ")|*." + Serializer.fileExtension;

            FilepathCurrent = null;
            Loading = false;

            RecentFiles = new RecentFilesList(recentFiles, this);
        }

        private void ControllerEditor_LevelSaved(ControllerEditor controller, string filepath)
        {
            ControllerWPF.Dispatcher.Invoke((Action)(() =>
            {
                RecentFiles.AddFilepath(filepath);
            }));
        }

        /*private void ControllerEditor_LevelLoaded(ControllerEditor controller, string filepath)
        {
            
        }*/

        private void ControllerEditor_LevelLoaded(ControllerEditor controller, string filepath)
        {
            ControllerWPF.Dispatcher.Invoke((Action)(() =>
            {
                Debug.Assert(Loading);
                RecentFiles.AddFilepath(filepath);
                Loading = false;
            }));
            
            
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
                ControllerEditor.AddAction(() =>
                {
                    ControllerEditor.LevelNew();
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
                _loadFileDialog.ShowDialog();
            }
        }

        private void _loadFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Load(((OpenFileDialog)sender).FileName);
        }

        private void _saveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
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
                ControllerEditor.AddAction(() =>
                {
                    ControllerEditor.LevelSave(filepath);
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
                FilepathCurrent = filepath;
                Loading = true;
                ControllerEditor.AddAction(() =>
                {
                    //ControllerEditor.LevelNew();
                    ControllerEditor.LevelLoad(filepath);
                });
            }
        }
    }
}
