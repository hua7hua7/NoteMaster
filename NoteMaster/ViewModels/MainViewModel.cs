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
            // TODO: 打开便签编辑窗口
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