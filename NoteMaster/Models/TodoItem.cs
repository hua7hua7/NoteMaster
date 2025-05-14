using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace NoteMaster.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _content = string.Empty;
        private bool _isCompleted;
        private DateTime _createdAt = DateTime.Now;
        private DateTime _updatedAt = DateTime.Now;
        private DateTime? _dueDate;
        private DateTime? _reminderAt;
        private bool _isReminderSet;
        private DateTime? _lastReminderTime;
        private int _reminderCount;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Content { get => _content; set => SetProperty(ref _content, value); }
        public DateTime CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }
        public DateTime UpdatedAt { get => _updatedAt; set => SetProperty(ref _updatedAt, value); }
        public DateTime? DueDate { get => _dueDate; set => SetProperty(ref _dueDate, value); }
        public DateTime? LastReminderTime { get => _lastReminderTime; set => SetProperty(ref _lastReminderTime, value); }
        public int ReminderCount { get => _reminderCount; set => SetProperty(ref _reminderCount, value); }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value))
                {
                    OnPropertyChanged(nameof(DisplayTextColor));
                    OnPropertyChanged(nameof(DisplayTextDecorations));
                    OnPropertyChanged(nameof(IsReminderSet));

                    if (_isCompleted && _isReminderSet)
                    {
                        ReminderAt = null;
                    }
                }
            }
        }

        public DateTime? ReminderAt
        {
            get => _reminderAt;
            set
            {
                if (_reminderAt != value)
                {
                    _reminderAt = value;
                    OnPropertyChanged(nameof(ReminderAt));

                    bool newIsReminderSetState = _reminderAt.HasValue;
                    if (_isReminderSet != newIsReminderSetState)
                    {
                        _isReminderSet = newIsReminderSetState;
                        OnPropertyChanged(nameof(IsReminderSet));
                    }
                }
            }
        }

        public bool IsReminderSet
        {
            get => _reminderAt.HasValue; // Directly reflect ReminderAt
            set
            {
                if (_isReminderSet != value)
                {
                    _isReminderSet = value;
                    OnPropertyChanged(nameof(IsReminderSet));

                    if (!_isReminderSet && _reminderAt.HasValue)
                    {
                        _reminderAt = null;
                        OnPropertyChanged(nameof(ReminderAt));
                    }
                    else if (_isReminderSet && !_reminderAt.HasValue)
                    {
                        _reminderAt = DateTime.Now;
                        OnPropertyChanged(nameof(ReminderAt));
                    }
                }
            }
        }

        public Brush DisplayTextColor => _isCompleted ? Brushes.Gray : Brushes.Black;
        public TextDecorationCollection? DisplayTextDecorations => _isCompleted ? TextDecorations.Strikethrough : null;

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
        }
    }
}