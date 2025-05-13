using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows;

namespace NoteMaster.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _content;
        private bool _isCompleted;
        private DateTime _createdAt = DateTime.Now;
        private DateTime _updatedAt = DateTime.Now;
        private DateTime? _dueDate;
        private DateTime? _reminderAt;
        private bool _isReminderSet;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        public DateTime? DueDate
        {
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }

        public DateTime? ReminderAt
        {
            get => _reminderAt;
            set => SetProperty(ref _reminderAt, value);
        }

        public bool IsReminderSet
        {
            get => _isReminderSet;
            set => SetProperty(ref _isReminderSet, value);
        }

        public Brush DisplayTextColor
        {
            get
            {
                try
                {
                    return IsCompleted ? Brushes.Gray : Brushes.Black;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DisplayTextColor 异常: {ex.Message}\n{ex.StackTrace}");
                    return Brushes.Black;
                }
            }
        }

        public TextDecorationCollection DisplayTextDecorations
        {
            get
            {
                try
                {
                    return IsCompleted ? TextDecorations.Strikethrough : null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DisplayTextDecorations 异常: {ex.Message}\n{ex.StackTrace}");
                    return null;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            if (propertyName == nameof(IsCompleted))
            {
                OnPropertyChanged(nameof(DisplayTextColor));
                OnPropertyChanged(nameof(DisplayTextDecorations));
            }
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}