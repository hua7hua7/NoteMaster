using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Linq;
using System.Windows;
using NoteMaster.Models;
using NoteMaster.Services;
using NoteMaster.Views;

namespace NoteMaster.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataStorageService _storageService;
        private string _searchQuery = string.Empty;
        private ObservableCollection<Note> _notes = new();
        private ObservableCollection<Folder> _folders = new();
        private Folder? _selectedFolder;
        private ObservableCollection<Note> _selectedNotes = new();

        public ObservableCollection<Note> Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
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

        public ObservableCollection<Note> SelectedNotes
        {
            get => _selectedNotes;
            set
            {
                _selectedNotes = value;
                OnPropertyChanged(nameof(SelectedNotes));
            }
        }

        public Folder? SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                _selectedFolder = value;
                OnPropertyChanged(nameof(SelectedFolder));
                FilterNotesByFolder();
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                FilterNotes();
            }
        }

        // 命令定义
        public ICommand CreateNoteCommand { get; }
        public ICommand CreateFolderCommand { get; }
        public ICommand DeleteFolderCommand { get; }
        public ICommand RenameFolderCommand { get; }
        public ICommand MoveNotesToFolderCommand { get; }
        public ICommand RemoveNotesFromFolderCommand { get; }
        public ICommand DeleteSelectedNotesCommand { get; }

        public MainViewModel()
        {
            _storageService = new DataStorageService();
            Notes = new ObservableCollection<Note>(_storageService.LoadNotes());
            Folders = new ObservableCollection<Folder>(_storageService.LoadFolders());
            SelectedNotes = new ObservableCollection<Note>();

            // 初始化命令
            CreateNoteCommand = new RelayCommand(CreateNote);
            CreateFolderCommand = new RelayCommand(CreateFolder);
            DeleteFolderCommand = new RelayCommand(DeleteFolder);
            RenameFolderCommand = new RelayCommand(RenameFolder);
            MoveNotesToFolderCommand = new RelayCommand(MoveNotesToFolder);
            RemoveNotesFromFolderCommand = new RelayCommand(RemoveNotesFromFolder);
            DeleteSelectedNotesCommand = new RelayCommand(DeleteSelectedNotes);
        }

        public void SaveNotes()
        {
            _storageService.SaveNotes(Notes.ToList());
            OnPropertyChanged(nameof(Notes));
        }

        private void CreateNote()
        {
            var newNote = new Note
            {
                Title = "New Note",
                Content = "",
                FolderId = SelectedFolder?.Id
            };
            Notes.Add(newNote);
            SaveNotes();

            var editWindow = new NoteEditWindow(newNote);
            editWindow.Closed += (s, e) =>
            {
                SaveNotes();
            };
            editWindow.ShowDialog();
        }

        private void CreateFolder()
        {
            var newFolder = new Folder
            {
                Name = "New Folder"
            };
            Folders.Add(newFolder);
            _storageService.SaveFolders(Folders.ToList());
            OnPropertyChanged(nameof(Folders));
        }

        private void DeleteFolder()
        {
            if (SelectedFolder == null) return;

            var result = MessageBox.Show(
                "删除文件夹时，是否同时删除其中的便签？\n选择是删除便签，选择否将便签移到根目录。",
                "确认删除",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel) return;

            var notesInFolder = Notes.Where(n => n.FolderId == SelectedFolder.Id).ToList();
            
            if (result == MessageBoxResult.Yes)
            {
                // 删除便签
                foreach (var note in notesInFolder)
                {
                    Notes.Remove(note);
                }
            }
            else
            {
                // 将便签移到根目录
                foreach (var note in notesInFolder)
                {
                    note.FolderId = null;
                }
            }

            Folders.Remove(SelectedFolder);
            SaveNotes();
            _storageService.SaveFolders(Folders.ToList());
            OnPropertyChanged(nameof(Folders));
        }

        private void RenameFolder()
        {
            if (SelectedFolder == null) return;

            var dialog = new RenameFolderDialog(SelectedFolder.Name);
            if (dialog.ShowDialog() == true)
            {
                SelectedFolder.Name = dialog.NewName;
                SelectedFolder.UpdatedAt = DateTime.Now;
                _storageService.SaveFolders(Folders.ToList());
                
                // 强制更新文件夹列表
                var currentFolders = Folders.ToList();
                Folders.Clear();
                foreach (var folder in currentFolders)
                {
                    Folders.Add(folder);
                }
                OnPropertyChanged(nameof(Folders));
            }
        }

        private void MoveNotesToFolder()
        {
            if (!SelectedNotes.Any()) return;

            var dialog = new SelectFolderDialog(Folders.ToList());
            if (dialog.ShowDialog() == true && dialog.SelectedFolder != null)
            {
                foreach (var note in SelectedNotes)
                {
                    note.FolderId = dialog.SelectedFolder.Id;
                }

                SaveNotes();
                FilterNotesByFolder(); // 立即更新显示
            }
        }

        private void RemoveNotesFromFolder()
        {
            if (!SelectedNotes.Any()) return;

            foreach (var note in SelectedNotes)
            {
                note.FolderId = null;
            }

            SaveNotes();
            FilterNotesByFolder(); // 立即更新显示
        }

        private void DeleteSelectedNotes()
        {
            if (!SelectedNotes.Any()) return;

            var result = MessageBox.Show(
                "确定要删除选中的便签吗？",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            foreach (var note in SelectedNotes.ToList())
            {
                Notes.Remove(note);
            }

            SaveNotes();
        }

        private void FilterNotesByFolder()
        {
            if (SelectedFolder == null)
            {
                var allNotes = _storageService.LoadNotes();
                Notes = new ObservableCollection<Note>(allNotes);
            }
            else
            {
                var filteredNotes = _storageService.LoadNotes()
                    .Where(n => n.FolderId == SelectedFolder.Id)
                    .ToList();
                Notes = new ObservableCollection<Note>(filteredNotes);
            }
            OnPropertyChanged(nameof(Notes));
        }

        private void FilterNotes()
        {
            var allNotes = _storageService.LoadNotes();
            IEnumerable<Note> filteredNotes = allNotes;

            if (SelectedFolder != null)
            {
                filteredNotes = filteredNotes.Where(n => n.FolderId == SelectedFolder.Id);
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                filteredNotes = filteredNotes.Where(n =>
                    n.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    n.Content.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                );
            }

            var notesList = filteredNotes.ToList();
            Notes = new ObservableCollection<Note>(notesList);
            OnPropertyChanged(nameof(Notes));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}