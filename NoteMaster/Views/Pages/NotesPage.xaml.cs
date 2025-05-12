using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NoteMaster.ViewModels;
using NoteMaster.Models;
using System.Text.RegularExpressions;

namespace NoteMaster.Views.Pages
{
    /// <summary>
    /// Lógica de interacción para NotesPage.xaml
    /// </summary>
    public partial class NotesPage : Page
    {
        private readonly ArchiveViewModel _viewModel;
        private static readonly Regex _invalidCharsRegex = new Regex($"[{Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()))}]");
        private string _originalFolderName;

        public NotesPage()
        {
            InitializeComponent();
            _viewModel = new ArchiveViewModel();
            DataContext = _viewModel;
        }

        private void FolderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is Folder selectedFolder)
            {
                _viewModel.SelectFolder(selectedFolder);
            }
        }

        private void FolderListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is Folder selectedFolder)
            {
                _viewModel.ViewFolder(selectedFolder);
            }
        }

        private void StartEditing(Folder folder)
        {
            if (folder == null) return;

            // 保存原始名称
            _originalFolderName = folder.Name;

            // 取消其他正在编辑的文件夹
            foreach (var f in _viewModel.Folders)
            {
                if (f != folder)
                {
                    f.IsEditing = false;
                }
            }
            folder.IsEditing = true;

            // 获取并聚焦到TextBox
            var container = FolderListBox.ItemContainerGenerator.ContainerFromItem(folder) as ListBoxItem;
            if (container != null)
            {
                var textBox = FindVisualChild<TextBox>(container);
                if (textBox != null)
                {
                    textBox.Focus();
                    textBox.SelectAll();
                }
            }
        }

        private void FolderNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is Folder folder)
            {
                if (e.Key == Key.Enter)
                {
                    SaveFolderName(folder, textBox.Text);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    CancelEditing(folder);
                    e.Handled = true;
                }
            }
        }

        private void FolderNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is Folder folder)
            {
                SaveFolderName(folder, textBox.Text);
            }
        }

        private void SaveFolderName(Folder folder, string newName)
        {
            if (folder == null) return;

            newName = newName.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("文件夹名称不能为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                folder.Name = _originalFolderName;
                folder.IsEditing = false;
                return;
            }

            if (newName.Length > 50)
            {
                MessageBox.Show("文件夹名称不能超过50个字符！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                folder.Name = _originalFolderName;
                folder.IsEditing = false;
                return;
            }

            if (_invalidCharsRegex.IsMatch(newName))
            {
                MessageBox.Show("文件夹名称包含非法字符！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                folder.Name = _originalFolderName;
                folder.IsEditing = false;
                return;
            }

            // 检查名称是否重复
            if (_viewModel.Folders.Any(f => f.Id != folder.Id && f.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("已存在同名文件夹！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                folder.Name = _originalFolderName;
                folder.IsEditing = false;
                return;
            }

            folder.Name = newName;
            folder.UpdatedAt = DateTime.Now;
            folder.IsEditing = false;
            _viewModel.SaveFolder(folder);
        }

        private void CancelEditing(Folder folder)
        {
            if (folder == null) return;
            folder.Name = _originalFolderName;
            folder.IsEditing = false;
        }

        private void NotesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                var selectedNotes = listBox.SelectedItems.Cast<Note>().ToList();
                _viewModel.SelectedNotes = new System.Collections.ObjectModel.ObservableCollection<Note>(selectedNotes);
            }
        }

        private void NotesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is Note selectedNote)
            {
                NavigationService?.Navigate(new NoteEditPage(selectedNote));
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T result)
                {
                    return result;
                }
                
                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                {
                    return descendant;
                }
            }
            return null;
        }
    }
}
