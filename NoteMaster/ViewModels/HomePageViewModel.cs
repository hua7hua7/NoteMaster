using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NoteMaster.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.RightsManagement;
using NoteMaster.Services;
using System.Windows.Input;

namespace NoteMaster.ViewModels
{
   public  class HomePageViewModel : INotifyPropertyChanged
    {
        //实现INotifyPropertyChanged接口
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private readonly DataStorageService _storageService;
        private ObservableCollection<Note> _notes = new();
        private ObservableCollection<Folder> _folders = new();
        private string _searchQuery = string.Empty;

        public ObservableCollection<Note> Notes
        {
            get => _notes;
            set
            {
                if(_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }

        public ObservableCollection<Folder> Folders
        {
            get => _folders;
            set
            {
                _folders = value;
                OnPropertyChanged(nameof(Folders));
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged(nameof(SearchQuery));
                }
            }
        }

        public ICommand SearchCommand => new RelayCommand(SearchNotes);
        public ICommand DeleteNoteCommand => new RelayCommand<Note>(DeleteNote);

        public HomePageViewModel()
        {
            _storageService = new DataStorageService();
            Notes = new ObservableCollection<Note>(_storageService.LoadNotes());
            //Folders = new ObservableCollection<Folder>(_storageService.LoadFolders());  
        }

        private void SearchNotes()
        {
            var allNotes = _storageService.LoadNotes();
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                Notes = new ObservableCollection<Note>(allNotes);
            }
            else
            {
                var filtered = allNotes.Where(n =>
                    (n.Title != null && n.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (n.Content != null && n.Content.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                Notes = new ObservableCollection<Note>(filtered);
            }
        }

        private void DeleteNote(Note note)
        {
            if (note == null) return;

            var notes = _storageService.LoadNotes();
            var noteToDelete = notes.FirstOrDefault(n => n.Id == note.Id);
            if (noteToDelete != null)
            {
                notes.Remove(noteToDelete);
                _storageService.SaveNotes(notes);
                Notes.Remove(note);
            }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        public RelayCommand(Action<T> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute((T)parameter);
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
