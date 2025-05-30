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
        // 数据存储服务，用于读写笔记和文件夹数据
        private readonly DataStorageService _storageService;

        // 所有笔记集合
        private ObservableCollection<Note> _notes = new();
        // 所有文件夹集合
        private ObservableCollection<Folder> _folders = new();

        // 当前查看的文件夹（右侧内容区域展示用）
        private Folder? _currentFolder;
        // 当前选中的文件夹（用于操作按钮）
        private Folder? _selectedFolder;

        // 当前界面显示的笔记集合（根据 currentFolder 决定内容）
        private ObservableCollection<Note> _displayedNotes = new();

        // 当前被选中的笔记集合（用于批量移动、移除等操作）
        private ObservableCollection<Note> _selectedNotes = new();

        // 输入框中用于新建文件夹的名称（默认空字符串）
        private string _newFolderName = string.Empty;

        // 属性变更通知事件
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // 所有笔记集合的属性封装
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

        // 所有文件夹集合的属性封装
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

        // 当前正在查看的文件夹
        public Folder? CurrentFolder
        {
            get => _currentFolder;
            private set
            {
                if (_currentFolder != value)
                {
                    _currentFolder = value;
                    OnPropertyChanged(nameof(CurrentFolder));
                    UpdateDisplayedNotes(); // 文件夹变更时刷新显示笔记
                }
            }
        }

        // 当前选中的文件夹（可供右键菜单使用）
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

        // 实际界面显示的笔记（过滤后结果）
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

        // 当前选中的多个笔记
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

        // 创建新文件夹的名称
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

        // 一系列命令绑定
        public ICommand CreateFolderCommand => new RelayCommand(CreateFolder);
        public ICommand DeleteFolderCommand => new RelayCommand(DeleteFolder);
        public ICommand RenameFolderCommand => new RelayCommand(RenameFolder);
        public ICommand CancelSelectFolderCommand => new RelayCommand(CancelSelectFolder);
        public ICommand MoveNotesToFolderCommand => new RelayCommand(MoveNotesToFolder);
        public ICommand RemoveNotesFromFolderCommand => new RelayCommand(RemoveNotesFromFolder);

        // 构造函数，加载数据
        public ArchiveViewModel()
        {
            _storageService = new DataStorageService();
            LoadData();
        }

        // 加载笔记和文件夹
        private void LoadData()
        {
            Notes = new ObservableCollection<Note>(_storageService.LoadNotes());
            Folders = new ObservableCollection<Folder>(_storageService.LoadFolders());
            UpdateDisplayedNotes();
        }

        // 设置选中的文件夹（不切换视图，仅用于操作）
        public void SelectFolder(Folder folder)
        {
            if (folder == null) return;
            SelectedFolder = folder;
        }

        // 切换当前查看的文件夹视图
        public void ViewFolder(Folder folder)
        {
            if (folder == null) return;
            CurrentFolder = folder;
            UpdateDisplayedNotes();
        }

        // 根据当前查看的文件夹更新界面上显示的笔记内容
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
                // 显示当前文件夹下的笔记
                DisplayedNotes = new ObservableCollection<Note>(
                    Notes.Where(n => n.FolderId == CurrentFolder.Id)
                );
            }

            // 清空选中的笔记
            SelectedNotes.Clear();
        }

        // 创建一个新文件夹
        private void CreateFolder()
        {
            var newFolder = new Folder
            {
                Name = "新建文件夹",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 设置唯一 ID
            newFolder.Id = Folders.Any() ? Folders.Max(f => f.Id) + 1 : 1;

            Folders.Add(newFolder);
            _storageService.SaveFolders(Folders.ToList());
            SelectedFolder = newFolder;
        }

        // 删除当前选中的文件夹，同时将其中笔记移出
        private void DeleteFolder()
        {
            if (SelectedFolder == null) return;

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

        // 进入文件夹重命名模式
        private void RenameFolder()
        {
            if (SelectedFolder == null) return;

            // 取消其他文件夹的编辑状态
            foreach (var folder in Folders)
            {
                folder.IsEditing = false;
            }

            SelectedFolder.IsEditing = true;
        }

        // 取消当前选中的文件夹和视图
        private void CancelSelectFolder()
        {
            SelectedFolder = null;
            CurrentFolder = null;
            UpdateDisplayedNotes();
        }

        // 将选中的笔记移动到选中的文件夹中
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

        // 将当前文件夹中的选中笔记移出文件夹（归档 -> 未归档）
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

        // 保存某个文件夹（通常用于重命名后）
        public void SaveFolder(Folder folder)
        {
            if (folder == null) return;
            folder.UpdatedAt = DateTime.Now;
            _storageService.SaveFolders(Folders.ToList());
            UpdateDisplayedNotes();
        }
    }
}
