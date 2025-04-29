using System;
using System.Windows;
using System.Windows.Controls;
using NoteMaster.ViewModels;
using NoteMaster.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace NoteMaster.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            try
            {
                InitializeComponent();
                _viewModel = viewModel;
                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"初始化主窗口时发生错误：\n{ex.Message}\n\n堆栈跟踪：\n{ex.StackTrace}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (e.NewValue is Folder folder)
                {
                    _viewModel.SelectedFolder = folder;
                }
                else
                {
                    _viewModel.SelectedFolder = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"选择文件夹时发生错误：\n{ex.Message}\n\n堆栈跟踪：\n{ex.StackTrace}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ListView listView)
                {
                    _viewModel.SelectedNotes = new ObservableCollection<Note>(
                        listView.SelectedItems.Cast<Note>()
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"选择便签时发生错误：\n{ex.Message}\n\n堆栈跟踪：\n{ex.StackTrace}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedNotes.Count != 1) return;

                var note = _viewModel.SelectedNotes[0];
                var editWindow = new NoteEditWindow(note);
                editWindow.Closed += (s, args) =>
                {
                    _viewModel.SaveNotes();
                };
                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"编辑便签时发生错误：\n{ex.Message}\n\n堆栈跟踪：\n{ex.StackTrace}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}