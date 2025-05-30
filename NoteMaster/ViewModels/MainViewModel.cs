using System;
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
    // 主视图模型，负责管理笔记应用的核心数据和逻辑，实现属性通知接口
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataStorageService _storageService;  // 数据存储服务实例，负责加载和保存数据

        // dev_A分支的变量，暂时注释，等待合并
        /*private string _searchQuery;
        private ObservableCollection<Note> _notes;
        private ObservableCollection<Note> _allNotes;*/ // 存储所有笔记的备份，方便搜索筛选使用

        // 当前搜索关键字，默认为空字符串
        private string _searchQuery = string.Empty;

        // 当前显示的笔记集合
        private ObservableCollection<Note> _notes = new();

        // 文件夹集合
        private ObservableCollection<Folder> _folders = new();

        // 当前选中的文件夹（可能为null表示根目录或未选择）
        private Folder? _selectedFolder;

        // 当前选中的笔记集合（多选）
        private ObservableCollection<Note> _selectedNotes = new();

        // 当前笔记列表的公开属性，支持数据绑定和通知更新
        public ObservableCollection<Note> Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        // 文件夹列表公开属性
        public ObservableCollection<Folder> Folders
        {
            get => _folders;
            set
            {
                _folders = value;
                OnPropertyChanged(nameof(Folders));
            }
        }

        // 当前选中的笔记集合，支持绑定多选操作
        public ObservableCollection<Note> SelectedNotes
        {
            get => _selectedNotes;
            set
            {
                _selectedNotes = value;
                OnPropertyChanged(nameof(SelectedNotes));
            }
        }

        // 当前选中文件夹，设置时自动触发笔记过滤
        public Folder? SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                _selectedFolder = value;
                OnPropertyChanged(nameof(SelectedFolder));
                FilterNotesByFolder(); // 切换文件夹时刷新笔记列表
            }
        }

        // 搜索框绑定的搜索关键字，变化时触发过滤操作
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                // dev_A的搜索操作，暂时注释
                //PerformSearch();
                FilterNotes();  // 关键字变化时刷新笔记列表
            }
        }

        // 命令定义：绑定到界面按钮，实现具体操作逻辑
        public ICommand CreateNoteCommand { get; }
        // dev_A分支命令暂时注释
        // public ICommand SearchCommand { get; }
        // public ICommand CloseCommand { get; }
        public ICommand CreateFolderCommand { get; }
        public ICommand DeleteFolderCommand { get; }
        public ICommand RenameFolderCommand { get; }
        public ICommand MoveNotesToFolderCommand { get; }
        public ICommand RemoveNotesFromFolderCommand { get; }
        public ICommand DeleteSelectedNotesCommand { get; }

        // 构造函数，初始化数据和命令绑定
        public MainViewModel()
        {
            _storageService = new DataStorageService();

            // 从存储加载笔记和文件夹
            Notes = new ObservableCollection<Note>(_storageService.LoadNotes());
            Folders = new ObservableCollection<Folder>(_storageService.LoadFolders());

            SelectedNotes = new ObservableCollection<Note>();

            // 初始化命令，绑定具体执行方法
            CreateNoteCommand = new RelayCommand(CreateNote);
            CreateFolderCommand = new RelayCommand(CreateFolder);
            DeleteFolderCommand = new RelayCommand(DeleteFolder);
            RenameFolderCommand = new RelayCommand(RenameFolder);
            MoveNotesToFolderCommand = new RelayCommand(MoveNotesToFolder);
            RemoveNotesFromFolderCommand = new RelayCommand(RemoveNotesFromFolder);
            DeleteSelectedNotesCommand = new RelayCommand(DeleteSelectedNotes);
        }

        // 保存当前笔记列表到存储中，并触发界面更新通知
        public void SaveNotes()
        {
            _storageService.SaveNotes(Notes.ToList());
            OnPropertyChanged(nameof(Notes));
        }

        // 新建笔记，默认标题为“New Note”，内容为空，所属文件夹为当前选中文件夹
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

            // 弹出笔记编辑窗口，编辑完成后保存
            var editWindow = new NoteEditWindow(newNote);
            editWindow.Closed += (s, e) =>
            {
                SaveNotes();
            };
            editWindow.ShowDialog();
        }

        // 新建文件夹，默认名称“New Folder”
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

        // 删除当前选中文件夹，提示用户是否连同文件夹内笔记一并删除
        private void DeleteFolder()
        {
            if (SelectedFolder == null) return;

            // 弹窗确认删除操作
            var result = MessageBox.Show(
                "删除文件夹时，是否同时删除其中的便签？\n选择是删除便签，选择否将便签移到根目录。",
                "确认删除",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel) return;

            // 找到所有属于该文件夹的笔记
            var notesInFolder = Notes.Where(n => n.FolderId == SelectedFolder.Id).ToList();

            if (result == MessageBoxResult.Yes)
            {
                // 用户选择删除文件夹内的便签，逐个移除
                foreach (var note in notesInFolder)
                {
                    Notes.Remove(note);
                }
            }
            else
            {
                // 用户选择保留便签，将它们的文件夹ID设为null，移至根目录
                foreach (var note in notesInFolder)
                {
                    note.FolderId = null;
                }
            }

            // 删除文件夹
            Folders.Remove(SelectedFolder);

            SaveNotes();
            _storageService.SaveFolders(Folders.ToList());
            OnPropertyChanged(nameof(Folders));
        }

        // 重命名选中文件夹，弹出重命名对话框进行输入
        private void RenameFolder()
        {
            if (SelectedFolder == null) return;

            var dialog = new RenameFolderDialog(SelectedFolder.Name);
            if (dialog.ShowDialog() == true)
            {
                // 更新文件夹名称和更新时间
                SelectedFolder.Name = dialog.NewFolderName;
                SelectedFolder.UpdatedAt = DateTime.Now;

                _storageService.SaveFolders(Folders.ToList());

                // 重新刷新文件夹集合，强制触发界面更新
                var currentFolders = Folders.ToList();
                Folders.Clear();
                foreach (var folder in currentFolders)
                {
                    Folders.Add(folder);
                }
                OnPropertyChanged(nameof(Folders));
            }
        }

        // 将选中的笔记移动到指定文件夹，弹出选择文件夹对话框
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
                FilterNotesByFolder(); // 立即刷新显示
            }
        }

        // 从文件夹中移除选中的笔记（移至根目录）
        private void RemoveNotesFromFolder()
        {
            if (!SelectedNotes.Any()) return;

            foreach (var note in SelectedNotes)
            {
                note.FolderId = null;
            }

            SaveNotes();
            FilterNotesByFolder(); // 立即刷新显示
        }

        // 删除选中的笔记，弹窗确认后执行删除操作
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

        // 根据当前选中文件夹过滤笔记列表，显示对应文件夹的笔记或全部笔记
        private void FilterNotesByFolder()
        {
            if (SelectedFolder == null)
            {
                // 未选择文件夹，显示所有笔记
                var allNotes = _storageService.LoadNotes();
                Notes = new ObservableCollection<Note>(allNotes);
            }
            else
            {
                // 只显示当前文件夹内的笔记
                var filteredNotes = _storageService.LoadNotes()
                    .Where(n => n.FolderId == SelectedFolder.Id)
                    .ToList();
                Notes = new ObservableCollection<Note>(filteredNotes);
            }
            OnPropertyChanged(nameof(Notes));
        }

        // 根据搜索关键字和文件夹筛选笔记，实现模糊搜索
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
                // 搜索标题或内容包含关键字的笔记，忽略大小写
                filteredNotes = filteredNotes.Where(n =>
                    n.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    n.Content.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                );
            }

            var notesList = filteredNotes.ToList();
            Notes = new ObservableCollection<Note>(notesList);
            OnPropertyChanged(nameof(Notes));
        }

        // 属性变更事件声明，实现INotifyPropertyChanged接口
        public event PropertyChangedEventHandler? PropertyChanged;

        // 触发属性变更通知，刷新绑定界面
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 命令模式实现类，实现ICommand接口，用于绑定按钮等UI控件的行为
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;          // 命令执行委托
        private readonly Func<object, bool> _canExecute;   // 是否可执行判断委托（可选）

        // 构造函数，接收执行委托和可执行判断委托
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // 重载，支持无参数版本
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(o => execute(), canExecute == null ? (Func<object, bool>)null : o => canExecute())
        {
        }

        // 判断命令是否可执行
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        // 执行命令
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // 当命令的可执行状态发生变化时触发，通知WPF更新绑定控件状态
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // 手动触发命令状态重新评估
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
