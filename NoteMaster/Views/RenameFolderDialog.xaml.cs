using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace NoteMaster.Views
{
    public partial class RenameFolderDialog : Window
    {
        public string NewFolderName { get; private set; }
        private static readonly Regex _invalidCharsRegex = new Regex($"[{Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()))}]");

        public RenameFolderDialog(string currentName = "")
        {
            InitializeComponent();
            FolderNameTextBox.Text = currentName;
            FolderNameTextBox.SelectAll();
            FolderNameTextBox.Focus();

            // 添加回车键支持
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    OkButton_Click(s, e);
                }
                else if (e.Key == Key.Escape)
                {
                    CancelButton_Click(s, e);
                }
            };

            // 添加输入验证
            FolderNameTextBox.TextChanged += (s, e) =>
            {
                string text = FolderNameTextBox.Text;
                if (text.Length > 50)
                {
                    FolderNameTextBox.Text = text.Substring(0, 50);
                    FolderNameTextBox.SelectionStart = 50;
                }
            };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string newName = FolderNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("文件夹名称不能为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                FolderNameTextBox.Focus();
                return;
            }

            if (newName.Length > 50)
            {
                MessageBox.Show("文件夹名称不能超过50个字符！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                FolderNameTextBox.Focus();
                return;
            }

            if (_invalidCharsRegex.IsMatch(newName))
            {
                MessageBox.Show("文件夹名称包含非法字符！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                FolderNameTextBox.Focus();
                return;
            }

            NewFolderName = newName;
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