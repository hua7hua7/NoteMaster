using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using NoteMaster.Models;
using NoteMaster.Services;

namespace NoteMaster.ViewModels
{
    public class ArchiveViewModel : INotifyPropertyChanged
    {
        private readonly DataStorageService _storageService;
        private ObservableCollection<Note> _notes = new();
        private ObservableCollection<Folder> _folders = new();
        private Folder? _currentFolder;
        private Folder? _selectedFolder;
        private ObservableCollection<Note> _displayedNotes = new();
        private ObservableCollection<Note> _selectedNotes = new();
        private string _newFolderName = string.Empty;

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
            private set
            {
                if (_currentFolder != value)
                {
                    _currentFolder = value;
                    OnPropertyChanged(nameof(CurrentFolder));
                    UpdateDisplayedNotes();
                }
            }
        }

        public Folder? SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (_selectedFolder != value)
                {
                    _selectedFolder = value;
                    OnPropertyChanged(nameof(SelectedFolder));
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

        public ObservableCollection<Note> SelectedNotes
        {
            get => _selectedNotes;
            set
            {
                if (_selectedNotes != value)
                {
                    _selectedNotes = value;
                    OnPropertyChanged(nameof(SelectedNotes));
                }
            }
        }

        public string NewFolderName
        {
            get => _newFolderName;
            set
            {
                if (_newFolderName != value)
                {
                    _newFolderName = value;
                    OnPropertyChanged(nameof(NewFolderName));
                }
            }
        }

        public ICommand CreateFolderCommand => new RelayCommand(CreateFolder);
        public ICommand DeleteFolderCommand => new RelayCommand(DeleteFolder);
        public ICommand RenameFolderCommand => new RelayCommand(RenameFolder);
        public ICommand CancelSelectFolderCommand => new RelayCommand(CancelSelectFolder);
        public ICommand MoveNotesToFolderCommand => new RelayCommand(MoveNotesToFolder);
        public ICommand RemoveNotesFromFolderCommand => new RelayCommand(RemoveNotesFromFolder);

        public ArchiveViewModel()
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

        public void SelectFolder(Folder folder)
        {
            if (folder == null) return;
            SelectedFolder = folder;
        }

        public void ViewFolder(Folder folder)
        {
            if (folder == null) return;
            CurrentFolder = folder;
            UpdateDisplayedNotes();
        }

        private void UpdateDisplayedNotes()
        {
            if (CurrentFolder == null)
            {
                // 显示未归档的笔记
                DisplayedNotes = new ObservableCollection<Note>(
                    Notes.Where(n => n.FolderId == null)
                );
            }
            else
            {
                // 显示当前文件夹的笔记
                DisplayedNotes = new ObservableCollection<Note>(
                    Notes.Where(n => n.FolderId == CurrentFolder.Id)
                );
            }
            SelectedNotes.Clear();
        }

        private void CreateFolder()
        {
            var newFolder = new Folder
            {
                Name = "新建文件夹",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 确保ID是唯一的
            if (Folders.Any())
            {
                newFolder.Id = Folders.Max(f => f.Id) + 1;
            }
            else
            {
                newFolder.Id = 1;
            }

            Folders.Add(newFolder);
            _storageService.SaveFolders(Folders.ToList());
            SelectedFolder = newFolder;
        }

        private void DeleteFolder()
        {
            if (SelectedFolder == null) return;

            // 处理文件夹中的笔记
            var notesInFolder = Notes.Where(n => n.FolderId == SelectedFolder.Id).ToList();
            foreach (var note in notesInFolder)
            {
                note.FolderId = null;
                note.UpdatedAt = DateTime.Now;
            }

            Folders.Remove(SelectedFolder);
            _storageService.SaveFolders(Folders.ToList());
            _storageService.SaveNotes(Notes.ToList());

            if (CurrentFolder == SelectedFolder)
            {
                CurrentFolder = null;
            }
            SelectedFolder = null;
            UpdateDisplayedNotes();
        }

        private void RenameFolder()
        {
            if (SelectedFolder == null) return;

            // 取消其他正在编辑的文件夹
            foreach (var folder in Folders)
            {
                if (folder != SelectedFolder)
                {
                    folder.IsEditing = false;
                }
            }

            // 开始编辑选中的文件夹
            SelectedFolder.IsEditing = true;
        }

        private void CancelSelectFolder()
        {
            SelectedFolder = null;
            CurrentFolder = null;
            UpdateDisplayedNotes();
        }

        private void MoveNotesToFolder()
        {
            if (SelectedFolder == null || SelectedNotes.Count == 0) return;

            foreach (var note in SelectedNotes)
            {
                note.FolderId = SelectedFolder.Id;
                note.UpdatedAt = DateTime.Now;
            }

            _storageService.SaveNotes(Notes.ToList());
            UpdateDisplayedNotes();
        }

        private void RemoveNotesFromFolder()
        {
            if (CurrentFolder == null || SelectedNotes.Count == 0) return;

            foreach (var note in SelectedNotes)
            {
                note.FolderId = null;
                note.UpdatedAt = DateTime.Now;
            }

            _storageService.SaveNotes(Notes.ToList());
            UpdateDisplayedNotes();
        }

        public void SaveFolder(Folder folder)
        {
            if (folder == null) return;
            folder.UpdatedAt = DateTime.Now;
            _storageService.SaveFolders(Folders.ToList());
            UpdateDisplayedNotes();
        }
    }
} 