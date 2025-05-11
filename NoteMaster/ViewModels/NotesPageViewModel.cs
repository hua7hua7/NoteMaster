using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using NoteMaster.Models;
using NoteMaster.Services;

namespace NoteMaster.ViewModels
{
    public class NotesPageViewModel : INotifyPropertyChanged
    {
        private readonly DataStorageService _storageService;
        private ObservableCollection<Note> _notes = new();
        private ObservableCollection<Folder> _folders = new();
        private Folder? _currentFolder;
        private ObservableCollection<Note> _displayedNotes = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ObservableCollection<Note> Notes
        {
            get => _notes;
            private set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }

        public ObservableCollection<Folder> Folders
        {
            get => _folders;
            private set
            {
                if (_folders != value)
                {
                    _folders = value;
                    OnPropertyChanged(nameof(Folders));
                }
            }
        }

        public Folder? CurrentFolder
        {
            get => _currentFolder;
            set
            {
                if (_currentFolder != value)
                {
                    _currentFolder = value;
                    OnPropertyChanged(nameof(CurrentFolder));
                    UpdateDisplayedNotes();
                }
            }
        }

        public ObservableCollection<Note> DisplayedNotes
        {
            get => _displayedNotes;
            private set
            {
                if (_displayedNotes != value)
                {
                    _displayedNotes = value;
                    OnPropertyChanged(nameof(DisplayedNotes));
                }
            }
        }

        public ICommand CreateFolderCommand => new RelayCommand(CreateFolder);
        public ICommand DeleteFolderCommand => new RelayCommand(DeleteFolder);
        public ICommand RenameFolderCommand => new RelayCommand(RenameFolder);
        public ICommand CreateNoteCommand => new RelayCommand(CreateNote);
        public ICommand DeleteNoteCommand => new RelayCommand<Note>(DeleteNote);
        public ICommand MoveNoteCommand => new RelayCommand<Note>(MoveNote);

        public NotesPageViewModel()
        {
            _storageService = new DataStorageService();
            LoadData();
        }

        private void LoadData()
        {
            Notes = new ObservableCollection<Note>(_storageService.LoadNotes());
            Folders = new ObservableCollection<Folder>(_storageService.LoadFolders());
            UpdateDisplayedNotes();
        }

        private void UpdateDisplayedNotes()
        {
            if (CurrentFolder == null)
            {
                DisplayedNotes = new ObservableCollection<Note>(Notes);
            }
            else
            {
                DisplayedNotes = new ObservableCollection<Note>(
                    Notes.Where(n => n.FolderId == CurrentFolder.Id)
                );
            }
        }

        private void CreateFolder()
        {
            // TODO: 实现创建文件夹功能
        }

        private void DeleteFolder()
        {
            // TODO: 实现删除文件夹功能
        }

        private void RenameFolder()
        {
            // TODO: 实现重命名文件夹功能
        }

        private void CreateNote()
        {
            // TODO: 实现创建笔记功能
        }

        private void DeleteNote(Note note)
        {
            if (note == null) return;

            Notes.Remove(note);
            _storageService.SaveNotes(Notes.ToList());
            UpdateDisplayedNotes();
        }

        private void MoveNote(Note note)
        {
            // TODO: 实现移动笔记功能
        }
    }
} 