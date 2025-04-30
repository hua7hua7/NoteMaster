using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NoteMaster.Models;
using NoteMaster.Services;

namespace NoteMaster.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataStorageService _storageService;
        private string _searchQuery;
        private ObservableCollection<Note> _notes;
        private ObservableCollection<Note> _allNotes; // 存储所有笔记的备份

        public ObservableCollection<Note> Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                PerformSearch();
            }
        }

        public ICommand CreateNoteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CloseCommand { get; }

        public MainViewModel()
        {
            _storageService = new DataStorageService();
            _allNotes = new ObservableCollection<Note>(_storageService.LoadNotes());
            Notes = new ObservableCollection<Note>(_allNotes);
            
            CreateNoteCommand = new RelayCommand(CreateNote);
            SearchCommand = new RelayCommand(ShowSearchDialog);
            CloseCommand = new RelayCommand(CloseApplication);
        }

        private void CreateNote()
        {
            // 创建新笔记的逻辑
            var newNote = new Note
            {
                Title = "新建笔记",
                Content = "",
                CreatedAt = DateTime.Now
            };
            
            Notes.Insert(0, newNote);
            _allNotes.Insert(0, newNote);
            
            // TODO: 打开笔记编辑窗口
            MessageBox.Show("新建笔记成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowSearchDialog()
        {
            // 显示搜索对话框
            var searchWindow = new Window
            {
                Title = "搜索笔记",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // TODO: 实现搜索对话框UI
            MessageBox.Show("搜索功能已启动！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                Notes = new ObservableCollection<Note>(_allNotes);
            }
            else
            {
                var searchResults = _allNotes.Where(note =>
                    note.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    note.Content.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                Notes = new ObservableCollection<Note>(searchResults);
            }
        }

        private void CloseApplication()
        {
            // 关闭应用程序
            Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ������ʵ��
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged;
    }
}