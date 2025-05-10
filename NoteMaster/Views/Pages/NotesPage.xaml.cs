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

namespace NoteMaster.Views.Pages
{
    /// <summary>
    /// Lógica de interacción para NotesPage.xaml
    /// </summary>
    public partial class NotesPage : Page
    {
        private readonly ArchiveViewModel _viewModel;
        private NoteMaster.Models.Folder? _pendingSelectedFolder = null;

        public NotesPage()
        {
            InitializeComponent();
            _viewModel = new ArchiveViewModel();
            DataContext = _viewModel;
        }

        private void FolderListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.CurrentFolder != null)
            {
                _viewModel.SelectFolder(_viewModel.CurrentFolder);
            }
        }

        private void NotesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ArchiveViewModel vm && sender is ListBox lb)
            {
                vm.SelectedNotes = new System.Collections.ObjectModel.ObservableCollection<NoteMaster.Models.Note>(lb.SelectedItems.Cast<NoteMaster.Models.Note>());
            }
        }
    }
}
