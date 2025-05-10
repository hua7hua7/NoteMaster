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

namespace NoteMaster.ViewModels
{
    public class ArchiveViewModel : INotifyPropertyChanged
    {
        private readonly DataStorageService _storageService;
        private ObservableCollection<Folder> _folders = new();
        private Folder? _selectedFolder;
        private ObservableCollection<Note> _notes = new();
        private ObservableCollection<Note> _selectedNotes = new();
        private Folder? _currentFolder;

        public ObservableCollection<Folder> Folders
        {
            get => _folders;
            set
            {
                _folders = value;
                OnPropertyChanged(nameof(Folders));
            }
        }

        public Folder? SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                _selectedFolder = value;
                OnPropertyChanged(nameof(SelectedFolder));
                OnPropertyChanged(nameof(DisplayedNotes));
            }
        }

        public Folder? CurrentFolder
        {
            get => _currentFolder;
            set
            {
                _currentFolder = value;
                OnPropertyChanged(nameof(CurrentFolder));
            }
        }

        public ObservableCollection<Note> DisplayedNotes
        {
            get
            {
                var allNotes = _storageService.LoadNotes();
                if (SelectedFolder == null)
                    return new ObservableCollection<Note>(allNotes.Where(n => n.FolderId == null));
                else
                    return new ObservableCollection<Note>(allNotes.Where(n => n.FolderId == SelectedFolder.Id));
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

        public ICommand CreateFolderCommand { get; }
        public ICommand DeleteFolderCommand { get; }
        public ICommand RenameFolderCommand { get; }
        public ICommand CancelSelectFolderCommand { get; }
        public ICommand MoveNotesToFolderCommand { get; }
        public ICommand RemoveNotesFromFolderCommand { get; }

        public ArchiveViewModel()
        {
            _storageService = new DataStorageService();
            Folders = new ObservableCollection<Folder>(_storageService.LoadFolders());
            _notes = new ObservableCollection<Note>(_storageService.LoadNotes());

            CreateFolderCommand = new RelayCommand(CreateFolder);
            DeleteFolderCommand = new RelayCommand(DeleteFolder);
            RenameFolderCommand = new RelayCommand(RenameFolder);
            CancelSelectFolderCommand = new RelayCommand(CancelSelectFolder);
            MoveNotesToFolderCommand = new RelayCommand(MoveNotesToFolder);
            RemoveNotesFromFolderCommand = new RelayCommand(RemoveNotesFromFolder);
        }

        public void SelectFolder(Folder folder)
        {
            SelectedFolder = folder;
        }

        private void CancelSelectFolder()
        {
            SelectedFolder = null;
        }

        private void CreateFolder()
        {
            var newFolder = new Folder
            {
                Id = Folders.Count + 1,
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

            var allNotes = _storageService.LoadNotes();
            var notesInFolder = allNotes.Where(n => n.FolderId == SelectedFolder.Id).ToList();
            if (result == MessageBoxResult.Yes)
            {
                foreach (var note in notesInFolder)
                {
                    allNotes.Remove(note);
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
            _storageService.SaveNotes(allNotes);
            OnPropertyChanged(nameof(Folders));
            OnPropertyChanged(nameof(DisplayedNotes));
            SelectedFolder = null;
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
                OnPropertyChanged(nameof(Folders));
            }
        }

        private void MoveNotesToFolder()
        {
            if (CurrentFolder == null || SelectedNotes == null || SelectedNotes.Count == 0) return;
            var allNotes = _storageService.LoadNotes();
            foreach (var note in SelectedNotes)
            {
                var n = allNotes.FirstOrDefault(x => x.Id == note.Id);
                if (n != null)
                {
                    n.FolderId = CurrentFolder.Id;
                }
            }
            _storageService.SaveNotes(allNotes);
            OnPropertyChanged(nameof(DisplayedNotes));
        }

        private void RemoveNotesFromFolder()
        {
            if (SelectedNotes == null || SelectedNotes.Count == 0) return;
            var allNotes = _storageService.LoadNotes();
            foreach (var note in SelectedNotes)
            {
                var n = allNotes.FirstOrDefault(x => x.Id == note.Id);
                if (n != null)
                {
                    n.FolderId = null;
                }
            }
            _storageService.SaveNotes(allNotes);
            OnPropertyChanged(nameof(DisplayedNotes));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 