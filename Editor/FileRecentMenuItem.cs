using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Editor
{
    public class FileRecentMenuItem : MenuItem
    {
        public FileRecentMenuItem(string filepath)
        {
            Header = filepath;
            Click += FileRecentMenuItem_Click;
        }

        private void FileRecentMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
