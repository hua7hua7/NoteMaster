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
        // 数据存储服务，用于加载和保存待办事项
        private readonly DataStorageService _storageService;

        // 新增待办事项的内容
        private string _newTodoContent = string.Empty;
        // 新增待办事项的截止日期（可为空）
        private DateTime? _newTodoDueDate;
        // 新增待办事项的提醒时间（字符串格式，HH:mm）
        private string _newTodoReminderTimeString = string.Empty;

        // 定时器，用于定期检查是否需要触发提醒
        private DispatcherTimer _reminderTimer;

        // 待办事项集合，供 UI 绑定显示
        private ObservableCollection<TodoItem> _todoItems;

        // 待办事项集合属性，支持数据绑定和属性更改通知
        public ObservableCollection<TodoItem> TodoItems
        {
            get => _todoItems;
            set
            {
                _todoItems = value;
                OnPropertyChanged(nameof(TodoItems));
            }
        }

        // 绑定的新待办事项内容属性
        public string NewTodoContent { get => _newTodoContent; set => SetProperty(ref _newTodoContent, value); }
        // 绑定的新待办事项截止日期属性
        public DateTime? NewTodoDueDate { get => _newTodoDueDate; set => SetProperty(ref _newTodoDueDate, value); }
        // 绑定的新待办事项提醒时间字符串属性（格式应为 HH:mm）
        public string NewTodoReminderTimeString { get => _newTodoReminderTimeString; set => SetProperty(ref _newTodoReminderTimeString, value); }

        // 命令，绑定到添加待办事项按钮
        public ICommand AddTodoCommand { get; }
        // 命令，绑定到删除待办事项按钮
        public ICommand DeleteTodoCommand { get; }
        // 命令，保存修改后的提醒时间
        public ICommand SaveItemReminderChangeCommand { get; }
        // 命令，清除某个待办事项的提醒
        public ICommand ClearItemReminderCommand { get; }

        // 构造函数，初始化服务、命令、加载数据、启动定时器
        public TodoPageViewModel()
        {
            _storageService = new DataStorageService();

            // 从存储中加载已有待办事项
            var loadedItems = _storageService.LoadTodoItems();
            TodoItems = new ObservableCollection<TodoItem>(loadedItems);
            SortTodoItems();

            // 初始化命令，并绑定对应的执行和状态判断方法
            AddTodoCommand = new RelayCommand(
                (object param) => AddTodoItem(param),
                (object param) => CanAddTodoItem()
            );

            DeleteTodoCommand = new RelayCommand((object param) => DeleteTodoItem(param));
            SaveItemReminderChangeCommand = new RelayCommand((object param) => SaveItemReminderChange(param));
            ClearItemReminderCommand = new RelayCommand((object param) => ClearItemReminder(param));

            // 初始化定时器，每15秒触发一次提醒检查
            _reminderTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
            _reminderTimer.Tick += ReminderTimer_Tick;
            _reminderTimer.Start();
        }

        // 判断是否允许添加待办事项（内容不能为空或空白）
        private bool CanAddTodoItem() => !string.IsNullOrWhiteSpace(NewTodoContent);

        /// <summary>
        /// 添加新的待办事项
        /// </summary>
        /// <param name="parameter">未使用</param>
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

            // 处理提醒时间字符串，验证格式并解析
            if (!string.IsNullOrWhiteSpace(NewTodoReminderTimeString))
            {
                // 正则匹配 HH:mm 格式
                if (Regex.IsMatch(NewTodoReminderTimeString, @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$"))
                {
                    // 解析时间字符串为 DateTime
                    if (DateTime.TryParseExact(NewTodoReminderTimeString, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
                    {
                        // 如果设置了截止日期，则提醒时间基于截止日期的日期部分 + 时间部分
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
                // 如果没设置提醒时间但设置了截止日期，默认提醒时间为截止日期当天上午9点
                newTodo.ReminderAt = NewTodoDueDate.Value.Date + new TimeSpan(9, 0, 0);
                Console.WriteLine($"AddTodoItem: Default ReminderAt set to {newTodo.ReminderAt}, IsReminderSet: {newTodo.IsReminderSet}");
            }
            else
            {
                // 未设置提醒时间和截止日期，则无提醒
                newTodo.ReminderAt = null;
                Console.WriteLine($"AddTodoItem: No reminder set, IsReminderSet: {newTodo.IsReminderSet}");
            }

            // 添加到集合，排序，保存到存储
            TodoItems.Add(newTodo);
            SortTodoItems();
            _storageService.SaveTodoItems(TodoItems.ToList());

            // 强制刷新 UI 绑定
            TodoItems = new ObservableCollection<TodoItem>(TodoItems);

            // 清空输入区域
            NewTodoContent = string.Empty;
            NewTodoDueDate = null;
            NewTodoReminderTimeString = string.Empty;
        }

        /// <summary>
        /// 保存单个待办事项的提醒时间修改
        /// </summary>
        /// <param name="parameter">目标待办事项对象</param>
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

        /// <summary>
        /// 清除待办事项的提醒时间
        /// </summary>
        /// <param name="parameter">目标待办事项对象</param>
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

        /// <summary>
        /// 删除指定的待办事项
        /// </summary>
        /// <param name="parameter">目标待办事项对象</param>
        private void DeleteTodoItem(object parameter)
        {
            if (parameter is TodoItem item)
            {
                TodoItems.Remove(item);
                _storageService.SaveTodoItems(TodoItems.ToList());
                // 强制刷新 UI
                TodoItems = new ObservableCollection<TodoItem>(TodoItems);
            }
        }

        /// <summary>
        /// 定时器事件，周期性检查是否有待办事项需要提醒
        /// </summary>
        private void ReminderTimer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            bool itemsThatNeedSavingOrResorting = false;

            foreach (var item in TodoItems.ToList())
            {
                // 条件：未完成、有设置提醒、提醒时间已到
                if (!item.IsCompleted && item.IsReminderSet && item.ReminderAt.HasValue && item.ReminderAt.Value <= now)
                {
                    // 检查是否第一次提醒，或距离上次提醒超过5分钟，且提醒次数未达3次
                    if (item.LastReminderTime == null || (now - item.LastReminderTime.Value).TotalMinutes >= 5 && item.ReminderCount < 3)
                    {
                        // 弹出提醒窗口（必须在 UI 线程调用）
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"提醒: \"{item.Content}\" 的时间到了！", "待办事项提醒", MessageBoxButton.OK, MessageBoxImage.Information);
                        });

                        // 记录提醒时间与次数
                        item.LastReminderTime = now;
                        item.ReminderCount++;
                        itemsThatNeedSavingOrResorting = true;
                        Console.WriteLine($"ReminderTimer_Tick: Triggered for {item.Content}, ReminderAt: {item.ReminderAt}, Count: {item.ReminderCount}");

                        // 超过3次提醒后自动取消提醒
                        if (item.ReminderCount >= 3)
                        {
                            item.ReminderAt = null;
                        }
                    }
                }
            }

            // 若有待办事项提醒状态发生变化，保存并刷新排序
            if (itemsThatNeedSavingOrResorting)
            {
                _storageService.SaveTodoItems(TodoItems.ToList());
                SortTodoItems();
                TodoItems = new ObservableCollection<TodoItem>(TodoItems);
            }
        }

        /// <summary>
        /// 对待办事项进行排序，保证未完成且设置提醒的项优先显示，按提醒时间和截止日期排序
        /// </summary>
        private void SortTodoItems()
        {
            var sortedItems = TodoItems
                .OrderBy(t => t.IsCompleted)               // 未完成优先
                .ThenBy(t => !t.ReminderAt.HasValue)      // 有提醒时间优先
                .ThenBy(t => t.ReminderAt)                 // 提醒时间早的优先
                .ThenBy(t => !t.DueDate.HasValue)          // 有截止日期优先
                .ThenBy(t => t.DueDate)                    // 截止日期早的优先
                .ThenByDescending(t => t.CreatedAt)       // 创建时间晚的优先
                .ToList();

            // 若顺序发生变化，更新集合以触发 UI 刷新
            if (!TodoItems.SequenceEqual(sortedItems))
            {
                TodoItems.Clear();
                foreach (var item in sortedItems) { TodoItems.Add(item); }
                TodoItems = new ObservableCollection<TodoItem>(TodoItems);
            }
        }

        // 属性更改通知事件
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 设置属性值并触发属性更改通知
        /// </summary>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 触发属性更改事件，通知 UI 更新绑定
        /// 同时当 NewTodoContent 变化时，刷新 AddTodoCommand 的可执行状态
        /// </summary>
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
