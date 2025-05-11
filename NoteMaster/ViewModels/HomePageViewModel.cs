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
        private string? _selectedTag;
        private ObservableCollection<string> _allTags = new();

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
                    Search();
                }
            }
        }

        public string? SelectedTag
        {
            get => _selectedTag;
            set
            {
                if (_selectedTag != value)
                {
                    _selectedTag = value;
                    OnPropertyChanged(nameof(SelectedTag));
                    Search();
                }
            }
        }

        public ObservableCollection<string> AllTags
        {
            get => _allTags;
            private set
            {
                if (_allTags != value)
                {
                    _allTags = value;
                    OnPropertyChanged(nameof(AllTags));
                }
            }
        }

        public ICommand SearchCommand => new RelayCommand(Search);
        public ICommand DeleteNoteCommand => new RelayCommand<Note>(DeleteNote);

        public HomePageViewModel()
        {
            _storageService = new DataStorageService();
            Notes = new ObservableCollection<Note>(_storageService.LoadNotes());
            //Folders = new ObservableCollection<Folder>(_storageService.LoadFolders());  
            LoadData();
        }

        private void LoadData()
        {
            var notes = _storageService.LoadNotes();
            _notes = new ObservableCollection<Note>(notes);

            var tags = notes.SelectMany(n => n.Tags ?? new List<string>())
                           .Distinct()
                           .OrderBy(t => t);
            _allTags = new ObservableCollection<string>(tags);
        }

        private void Search()
        {
            var notes = _storageService.LoadNotes();

            if (!string.IsNullOrEmpty(_selectedTag))
            {
                notes = notes.Where(n => n.Tags != null && n.Tags.Contains(_selectedTag)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                var searchLower = _searchQuery.ToLower();
                notes = notes.Where(n =>
                    (n.Title?.ToLower().Contains(searchLower) ?? false) ||
                    (n.Content?.ToLower().Contains(searchLower) ?? false) ||
                    (n.Tags?.Any(t => t.ToLower().Contains(searchLower)) ?? false)
                ).ToList();
            }

            Notes = new ObservableCollection<Note>(notes);
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
