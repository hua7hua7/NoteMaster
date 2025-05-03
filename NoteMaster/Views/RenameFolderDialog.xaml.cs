using System.Windows;

namespace NoteMaster.Views
{
    public partial class RenameFolderDialog : Window
    {
        public string NewName { get; set; }

        public RenameFolderDialog(string currentName)
        {
            InitializeComponent();
            NewName = currentName;
            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewName))
            {
                MessageBox.Show("文件夹名称不能为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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