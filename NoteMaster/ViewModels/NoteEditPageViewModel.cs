using System;
using System.ComponentModel;
using System.Windows.Input;
using NoteMaster.Models;
using NoteMaster.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace NoteMaster.ViewModels
{
    /// <summary>
    /// 笔记编辑页面的ViewModel，负责处理笔记的创建、编辑和删除功能
    /// </summary>
    public class NoteEditPageViewModel : INotifyPropertyChanged
    {
        // 私有字段
        private string _title = string.Empty;           // 笔记标题
        private string _content = string.Empty;         // 笔记内容
        private string _newTag = string.Empty;          // 新标签输入
        private readonly DataStorageService _storageService;  // 数据存储服务
        private Note? _editingNote;                     // 当前正在编辑的笔记
        private bool _isEditing;                        // 是否处于编辑模式
        private ObservableCollection<string> _tags;     // 标签集合

        // 实现INotifyPropertyChanged接口，用于通知UI属性变化
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 是否处于编辑模式（true表示编辑现有笔记，false表示新建笔记）
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            private set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }

        /// <summary>
        /// 笔记标题
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        /// <summary>
        /// 笔记内容
        /// </summary>
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }

        /// <summary>
        /// 新标签输入
        /// </summary>
        public string NewTag
        {
            get => _newTag;
            set
            {
                if (_newTag != value)
                {
                    _newTag = value;
                    OnPropertyChanged(nameof(NewTag));
                }
            }
        }

        /// <summary>
        /// 标签集合
        /// </summary>
        public ObservableCollection<string> Tags
        {
            get => _tags;
            private set
            {
                if (_tags != value)
                {
                    _tags = value;
                    OnPropertyChanged(nameof(Tags));
                }
            }
        }

        // 命令定义
        public ICommand SaveCommand { get; }    // 保存笔记命令
        public ICommand DeleteCommand { get; }  // 删除笔记命令

        // 事件定义
        public event EventHandler? NoteSaved;   // 笔记保存完成事件
        public event EventHandler? NoteDeleted; // 笔记删除完成事件

        /// <summary>
        /// 默认构造函数，用于创建新笔记
        /// </summary>
        public NoteEditPageViewModel() : this(null) { }

        /// <summary>
        /// 带参数的构造函数，用于编辑现有笔记
        /// </summary>
        /// <param name="note">要编辑的笔记，如果为null则表示创建新笔记</param>
        public NoteEditPageViewModel(Note? note)
        {
            _storageService = new DataStorageService();
            SaveCommand = new RelayCommand(SaveNote);
            DeleteCommand = new RelayCommand(DeleteNote);
            _tags = new ObservableCollection<string>();
            
            if (note != null)
            {
                // 编辑模式：加载现有笔记数据
                _editingNote = note;
                Title = note.Title;
                Content = note.Content;
                if (note.Tags != null)
                {
                    foreach (var tag in note.Tags)
                    {
                        _tags.Add(tag);
                    }
                }
                IsEditing = true;
            }
            else
            {
                // 新建模式：初始化空数据
                IsEditing = false;
            }
        }

        /// <summary>
        /// 添加标签
        /// </summary>
        /// <param name="tag">要添加的标签</param>
        public void AddTag(string tag)
        {
            if (!string.IsNullOrWhiteSpace(tag) && !_tags.Contains(tag))
            {
                _tags.Add(tag);
            }
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="tag">要删除的标签</param>
        public void RemoveTag(string tag)
        {
            if (_tags.Contains(tag))
            {
                _tags.Remove(tag);
            }
        }

        /// <summary>
        /// 保存笔记的方法
        /// </summary>
        private void SaveNote()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return;

            var notes = _storageService.LoadNotes();
            if (_editingNote != null)
            {
                // 编辑模式：更新现有笔记
                var note = notes.FirstOrDefault(n => n.Id == _editingNote.Id);
                if (note != null)
                {
                    note.Title = this.Title;
                    note.Content = this.Content;
                    note.Tags = this.Tags.ToList();
                    note.UpdatedAt = DateTime.Now;
                }
            }
            else
            {
                // 新建模式：创建新笔记
                var newNote = new Note
                {
                    Title = this.Title,
                    Content = this.Content,
                    Tags = this.Tags.ToList(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                notes.Add(newNote);
            }
            _storageService.SaveNotes(notes);
            NoteSaved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 删除笔记的方法
        /// </summary>
        private void DeleteNote()
        {
            if (_editingNote == null) return;

            var notes = _storageService.LoadNotes();
            var noteToDelete = notes.FirstOrDefault(n => n.Id == _editingNote.Id);
            if (noteToDelete != null)
            {
                notes.Remove(noteToDelete);
                _storageService.SaveNotes(notes);
                NoteDeleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
