using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using NoteMaster.Models;
using NoteMaster.Services;
using NoteMaster.Views;
using System.Windows;
using System.IO;

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
            SelectedFolder = folder;
            UpdateDisplayedNotes();
        }

        private void UpdateDisplayedNotes()
        {
            if (CurrentFolder == null)
            {
                DisplayedNotes = new ObservableCollection<Note>(
                    Notes.Where(n => n.FolderId == null)
                );
            }
            else
            {
                DisplayedNotes = new ObservableCollection<Note>(
                    Notes.Where(n => n.FolderId == CurrentFolder.Id)
                );
            }
            SelectedNotes.Clear();
        }

        private void CreateFolder()
        {
            var folder = new Folder 
            { 
                Name = "新建文件夹",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            Folders.Add(folder);
            _storageService.SaveFolders(Folders.ToList());
            SelectedFolder = folder;
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
                foreach (var note in notesInFolder)
                {
                    Notes.Remove(note);
                }
            }
            else
            {
                foreach (var note in notesInFolder)
                {
                    note.FolderId = null;
                }
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

            var dialog = new RenameFolderDialog(SelectedFolder.Name);
            if (dialog.ShowDialog() == true)
            {
                string newName = dialog.NewFolderName.Trim();
                
                // 检查名称是否为空
                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("文件夹名称不能为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 检查名称是否重复
                if (Folders.Any(f => f.Id != SelectedFolder.Id && f.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("已存在同名文件夹！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 检查名称长度
                if (newName.Length > 50)
                {
                    MessageBox.Show("文件夹名称不能超过50个字符！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 检查名称是否包含非法字符
                if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    MessageBox.Show("文件夹名称包含非法字符！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedFolder.Name = newName;
                SelectedFolder.UpdatedAt = DateTime.Now;
                _storageService.SaveFolders(Folders.ToList());
                
                // 强制更新UI
                var currentFolders = Folders.ToList();
                Folders.Clear();
                foreach (var folder in currentFolders)
                {
                    Folders.Add(folder);
                }
                OnPropertyChanged(nameof(Folders));
            }
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
    }
} 