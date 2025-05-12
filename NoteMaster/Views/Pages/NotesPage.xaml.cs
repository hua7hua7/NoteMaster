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

namespace NoteMaster.Views.Pages
{
    /// <summary>
    /// Lógica de interacción para NotesPage.xaml
    /// </summary>
    public partial class NotesPage : Page
    {
        private readonly ArchiveViewModel _viewModel;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
