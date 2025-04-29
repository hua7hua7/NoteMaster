using System.Collections.ObjectModel;
using System.ComponentModel;
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
                // TODO: 实现搜索逻辑
            }
        }

        public ICommand CreateNoteCommand { get; }

        public MainViewModel()
        {
            _storageService = new DataStorageService();
            Notes = new ObservableCollection<Note>(_storageService.LoadNotes());
            CreateNoteCommand = new RelayCommand(CreateNote);
        }

        private void CreateNote()
        {
             var newNote = new Note
            {
                Title = "New Note",
                Content = ""
            };
            Notes.Add(newNote);
            _storageService.SaveNotes(Notes.ToList()); // 保存到文件

             var editWindow = new NoteEditWindow(newNote);
            editWindow.Closed += (s, e) =>
            {
                _storageService.SaveNotes(Notes.ToList()); // 保存编辑后的笔记
                OnPropertyChanged(nameof(Notes)); // 刷新列表
            };
            editWindow.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 简单命令实现
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged;
    }
}