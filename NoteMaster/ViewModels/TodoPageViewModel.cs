using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NoteMaster.Models;
using NoteMaster.Services;

namespace NoteMaster.ViewModels
{
    public class TodoPageViewModel : INotifyPropertyChanged
    {
        private readonly DataStorageService _storageService;
        private string _newTodoContent = string.Empty;
        private DateTime? _newTodoDueDate;
        private string _newTodoReminderTimeString = string.Empty;
        private DispatcherTimer _reminderTimer;
        private ObservableCollection<TodoItem> _todoItems;

        public ObservableCollection<TodoItem> TodoItems
        {
            get => _todoItems;
            set
            {
                _todoItems = value;
                OnPropertyChanged(nameof(TodoItems));
            }
        }

        public string NewTodoContent { get => _newTodoContent; set => SetProperty(ref _newTodoContent, value); }
        public DateTime? NewTodoDueDate { get => _newTodoDueDate; set => SetProperty(ref _newTodoDueDate, value); }
        public string NewTodoReminderTimeString { get => _newTodoReminderTimeString; set => SetProperty(ref _newTodoReminderTimeString, value); }

        public ICommand AddTodoCommand { get; }
        public ICommand DeleteTodoCommand { get; }
        public ICommand SaveItemReminderChangeCommand { get; }
        public ICommand ClearItemReminderCommand { get; }

        public TodoPageViewModel()
        {
            _storageService = new DataStorageService();
            var loadedItems = _storageService.LoadTodoItems();
            TodoItems = new ObservableCollection<TodoItem>(loadedItems);
            SortTodoItems();

            AddTodoCommand = new RelayCommand(
                (object param) => AddTodoItem(param),
                (object param) => CanAddTodoItem()
            );

            DeleteTodoCommand = new RelayCommand((object param) => DeleteTodoItem(param));
            SaveItemReminderChangeCommand = new RelayCommand((object param) => SaveItemReminderChange(param));
            ClearItemReminderCommand = new RelayCommand((object param) => ClearItemReminder(param));

            _reminderTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
            _reminderTimer.Tick += ReminderTimer_Tick;
            _reminderTimer.Start();
        }

        private bool CanAddTodoItem() => !string.IsNullOrWhiteSpace(NewTodoContent);

        private void AddTodoItem(object parameter)
        {
            if (string.IsNullOrWhiteSpace(NewTodoContent))
            {
                MessageBox.Show("请输入待办事项内容！", "输入错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newTodo = new TodoItem
            {
                Content = NewTodoContent.Trim(),
                DueDate = NewTodoDueDate
            };

            if (!string.IsNullOrWhiteSpace(NewTodoReminderTimeString))
            {
                if (Regex.IsMatch(NewTodoReminderTimeString, @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$"))
                {
                    if (DateTime.TryParseExact(NewTodoReminderTimeString, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
                    {
                        DateTime reminderDateBase = NewTodoDueDate?.Date ?? DateTime.Today;
                        newTodo.ReminderAt = reminderDateBase + parsedTime.TimeOfDay;
                        Console.WriteLine($"AddTodoItem: ReminderAt set to {newTodo.ReminderAt}, IsReminderSet: {newTodo.IsReminderSet}");
                    }
                    else
                    {
                        MessageBox.Show($"提醒时间 \"{NewTodoReminderTimeString}\" 解析失败，请输入 HH:mm 格式（如 14:30）。", "格式错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                        newTodo.ReminderAt = null;
                    }
                }
                else
                {
                    MessageBox.Show($"提醒时间 \"{NewTodoReminderTimeString}\" 格式无效，请输入 HH:mm 格式（如 14:30）。", "格式错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    newTodo.ReminderAt = null;
                }
            }
            else if (NewTodoDueDate.HasValue)
            {
                newTodo.ReminderAt = NewTodoDueDate.Value.Date + new TimeSpan(9, 0, 0);
                Console.WriteLine($"AddTodoItem: Default ReminderAt set to {newTodo.ReminderAt}, IsReminderSet: {newTodo.IsReminderSet}");
            }
            else
            {
                newTodo.ReminderAt = null;
                Console.WriteLine($"AddTodoItem: No reminder set, IsReminderSet: {newTodo.IsReminderSet}");
            }

            TodoItems.Add(newTodo);
            SortTodoItems();
            _storageService.SaveTodoItems(TodoItems.ToList());
            TodoItems = new ObservableCollection<TodoItem>(TodoItems); // Force UI refresh

            NewTodoContent = string.Empty;
            NewTodoDueDate = null;
            NewTodoReminderTimeString = string.Empty;
        }

        private void SaveItemReminderChange(object parameter)
        {
            if (parameter is TodoItem item)
            {
                item.UpdatedAt = DateTime.Now;
                _storageService.SaveTodoItems(TodoItems.ToList());
                SortTodoItems();
                Console.WriteLine($"SaveItemReminderChange: ReminderAt: {item.ReminderAt}, IsReminderSet: {item.IsReminderSet}");
            }
        }

        private void ClearItemReminder(object parameter)
        {
            if (parameter is TodoItem item)
            {
                item.ReminderAt = null;
                item.UpdatedAt = DateTime.Now;
                _storageService.SaveTodoItems(TodoItems.ToList());
                SortTodoItems();
                Console.WriteLine($"ClearItemReminder: ReminderAt: {item.ReminderAt}, IsReminderSet: {item.IsReminderSet}");
            }
        }

        private void DeleteTodoItem(object parameter)
        {
            if (parameter is TodoItem item)
            {
                TodoItems.Remove(item);
                _storageService.SaveTodoItems(TodoItems.ToList());
                TodoItems = new ObservableCollection<TodoItem>(TodoItems);
            }
        }

        private void ReminderTimer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            bool itemsThatNeedSavingOrResorting = false;

            foreach (var item in TodoItems.ToList())
            {
                if (!item.IsCompleted && item.IsReminderSet && item.ReminderAt.HasValue && item.ReminderAt.Value <= now)
                {
                    if (item.LastReminderTime == null || (now - item.LastReminderTime.Value).TotalMinutes >= 5 && item.ReminderCount < 3)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"提醒: \"{item.Content}\" 的时间到了！", "待办事项提醒", MessageBoxButton.OK, MessageBoxImage.Information);
                        });

                        item.LastReminderTime = now;
                        item.ReminderCount++;
                        itemsThatNeedSavingOrResorting = true;
                        Console.WriteLine($"ReminderTimer_Tick: Triggered for {item.Content}, ReminderAt: {item.ReminderAt}, Count: {item.ReminderCount}");

                        if (item.ReminderCount >= 3)
                        {
                            item.ReminderAt = null;
                        }
                    }
                }
            }

            if (itemsThatNeedSavingOrResorting)
            {
                _storageService.SaveTodoItems(TodoItems.ToList());
                SortTodoItems();
                TodoItems = new ObservableCollection<TodoItem>(TodoItems);
            }
        }

        private void SortTodoItems()
        {
            var sortedItems = TodoItems
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => !t.ReminderAt.HasValue)
                .ThenBy(t => t.ReminderAt)
                .ThenBy(t => !t.DueDate.HasValue)
                .ThenBy(t => t.DueDate)
                .ThenByDescending(t => t.CreatedAt)
                .ToList();

            if (!TodoItems.SequenceEqual(sortedItems))
            {
                TodoItems.Clear();
                foreach (var item in sortedItems) { TodoItems.Add(item); }
                TodoItems = new ObservableCollection<TodoItem>(TodoItems);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(NewTodoContent))
            {
                if (AddTodoCommand is RelayCommand rc)
                {
                    rc.RaiseCanExecuteChanged();
                }
            }
        }
    }
}