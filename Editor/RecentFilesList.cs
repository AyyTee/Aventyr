using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EditorWindow
{
    public class RecentFilesList
    {
        public const int MaxSize = 10;
        public MenuItem RecentFilesDropdown;
        public List<Tuple<MenuItem, string>> ButtonList = new List<Tuple<MenuItem, string>>();
        public ControllerFiles ControllerFiles;

        public RecentFilesList(MenuItem recentFilesDropdown, ControllerFiles controllerFiles)
        {
            RecentFilesDropdown = recentFilesDropdown;
            ControllerFiles = controllerFiles;
            var filepaths = Properties.Settings.Default.RecentFilepaths;
            for (int i = Math.Min(MaxSize, filepaths.Count) - 1; i >= 0 ; i--)
            {
                InsertNewButton(filepaths[i]);
            }
            Update();
        }

        public void AddFilepath(string filepath)
        {
            var button = ButtonList.FirstOrDefault(item => item.Item2 == filepath);
            if (button != null)
            {
                ButtonList.Remove(button);
                ButtonList.Insert(0, button);
            }
            else
            {
                InsertNewButton(filepath);

                Debug.Assert(MaxSize > 0);
                while (RecentFilesDropdown.Items.Count > MaxSize)
                {
                    ButtonList.RemoveAt(ButtonList.Count - 1);
                    RecentFilesDropdown.Items.RemoveAt(RecentFilesDropdown.Items.Count - 1);
                }
            }
            Update();
        }

        void InsertNewButton(string filepath)
        {
            MenuItem buttonNew = new MenuItem();
            buttonNew.Click += Button_Click;
            ButtonList.Insert(0, new Tuple<MenuItem, string>(buttonNew, filepath));
            RecentFilesDropdown.Items.Insert(0, buttonNew);
        }

        void Update()
        {
            for (int i = 0; i < ButtonList.Count; i++)
            {
                ButtonList[i].Item1.Header = (i + 1).ToString() + " " + ButtonList[i].Item2;
            }
            Properties.Settings.Default.RecentFilepaths = GetStringList();
        }

        void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = ButtonList.Find(item => item.Item1 == sender);

            if (File.Exists(button.Item2))
            {
                ControllerFiles.Load(button.Item2);

                ButtonList.Remove(button);
                RecentFilesDropdown.Items.Remove(button.Item1);
                ButtonList.Insert(0, button);
                RecentFilesDropdown.Items.Insert(0, button.Item1);
            }
            else
            {
                ButtonList.Remove(button);
                RecentFilesDropdown.Items.Remove(button.Item1);
            }
            Update();
        }

        System.Collections.Specialized.StringCollection GetStringList()
        {
            var stringList = new System.Collections.Specialized.StringCollection();
            for (int i = 0; i < ButtonList.Count; i++)
            {
                stringList.Add(ButtonList[i].Item2);
            }
            return stringList;
        }
    }
}
