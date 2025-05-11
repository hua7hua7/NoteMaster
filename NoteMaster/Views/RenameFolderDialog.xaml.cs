using System.Windows;

namespace NoteMaster.Views
{
    public partial class RenameFolderDialog : Window
    {
        public string NewFolderName { get; private set; }

        public RenameFolderDialog(string currentName = "")
        {
            InitializeComponent();
            FolderNameTextBox.Text = currentName;
            FolderNameTextBox.SelectAll();
            FolderNameTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FolderNameTextBox.Text))
            {
                MessageBox.Show("文件夹名称不能为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewFolderName = FolderNameTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 