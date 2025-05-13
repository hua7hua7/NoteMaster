using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NoteMaster.Models;
using NoteMaster.Services;

namespace NoteMaster.ViewModels
{
    public class TodoPageViewModel : INotifyPropertyChanged
    {
        private readonly DataStorageService _storageService;
        private string _newTodoContent = string.Empty;
        private DateTime? _newTodoDueDate;

        public ObservableCollection<TodoItem> TodoItems { get; set; }

        public string NewTodoContent
        {
            get => _newTodoContent;
            set => SetProperty(ref _newTodoContent, value);
        }

        public DateTime? NewTodoDueDate
        {
            get => _newTodoDueDate;
            set => SetProperty(ref _newTodoDueDate, value);
        }

        public ICommand AddTodoCommand { get; }
        public ICommand ToggleCompleteCommand { get; }
        public ICommand DeleteTodoCommand { get; }
        public ICommand SetDueDateCommand { get; }

        public TodoPageViewModel()
        {
            try
            {
                _storageService = new DataStorageService();
                var loadedItems = _storageService.LoadTodoItems();
                // 过滤无效 TodoItem，确保 DueDate 是 DateTime?
                var validItems = loadedItems
                    .Where(item => item != null && (item.DueDate == null || item.DueDate is DateTime))
                    .OrderBy(t => t.CreatedAt)
                    .ToList();
                TodoItems = new ObservableCollection<TodoItem>(validItems);

                AddTodoCommand = new RelayCommand(AddTodoItem, _ => CanAddTodoItem());
                ToggleCompleteCommand = new RelayCommand(ToggleComplete);
                DeleteTodoCommand = new RelayCommand(DeleteTodoItem);
                SetDueDateCommand = new RelayCommand(UpdateDueDate);

                Console.WriteLine($"TodoPageViewModel 初始化成功，加载 {TodoItems.Count} 条待办事项");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TodoPageViewModel 初始化异常: {ex.Message}\n{ex.StackTrace}");
                TodoItems = new ObservableCollection<TodoItem>();
            }
        }

        private bool CanAddTodoItem()
        {
            return !string.IsNullOrWhiteSpace(NewTodoContent);
        }

        private void AddTodoItem(object parameter)
        {
            try
            {
                if (!CanAddTodoItem()) return;

                var newTodo = new TodoItem
                {
                    Content = NewTodoContent,
                    DueDate = NewTodoDueDate
                };
                TodoItems.Add(newTodo);
                SortTodoItems();
                _storageService.SaveTodoItems(TodoItems.ToList());

                NewTodoContent = string.Empty;
                NewTodoDueDate = null;
                Console.WriteLine($"添加待办项: {newTodo.Content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddTodoItem 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ToggleComplete(object parameter)
        {
            try
            {
                if (parameter is TodoItem item)
                {
                    item.IsCompleted = !item.IsCompleted;
                    _storageService.SaveTodoItems(TodoItems.ToList());
                    SortTodoItems();
                    Console.WriteLine($"切换待办项完成状态: {item.Content}, IsCompleted: {item.IsCompleted}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToggleComplete 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void DeleteTodoItem(object parameter)
        {
            try
            {
                if (parameter is TodoItem item)
                {
                    TodoItems.Remove(item);
                    _storageService.SaveTodoItems(TodoItems.ToList());
                    Console.WriteLine($"删除待办项: {item.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteTodoItem 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void UpdateDueDate(object parameter)
        {
            try
            {
                if (parameter is TodoItem item)
                {
                    item.UpdatedAt = DateTime.Now;
                    _storageService.SaveTodoItems(TodoItems.ToList());
                    SortTodoItems();
                    Console.WriteLine($"更新待办项截止日期: {item.Content}, DueDate: {item.DueDate}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateDueDate 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SortTodoItems()
        {
            try
            {
                var sortedItems = TodoItems
                    .OrderBy(t => t.IsCompleted)
                    .ThenBy(t => t.DueDate.HasValue ? 0 : 1)
                    .ThenBy(t => t.DueDate)
                    .ThenByDescending(t => t.CreatedAt)
                    .ToList();
                TodoItems.Clear();
                foreach (var item in sortedItems)
                {
                    TodoItems.Add(item);
                }
                Console.WriteLine("待办项排序完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SortTodoItems 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            try
            {
                if (Equals(storage, value)) return false;
                storage = value;
                OnPropertyChanged(propertyName);
                if (propertyName == nameof(NewTodoContent))
                {
                    (AddTodoCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetProperty 异常: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPropertyChanged 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}