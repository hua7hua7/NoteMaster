using System.Collections.Generic;
using System.Windows;
using NoteMaster.Models;

namespace NoteMaster.Views
{
    public partial class SelectFolderDialog : Window
    {
        public List<Folder> Folders { get; }
        public Folder? SelectedFolder { get; private set; }

        public SelectFolderDialog(List<Folder> folders)
        {
            InitializeComponent();
            Folders = folders;
            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFolder == null)
            {
                MessageBox.Show("请选择一个文件夹！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
} 