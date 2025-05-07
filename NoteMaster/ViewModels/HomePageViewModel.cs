using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NoteMaster.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.RightsManagement;

namespace NoteMaster.ViewModels
{
   public  class HomePageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Note> _notes;

        public ObservableCollection<Note> Notes
        {
            get => _notes;
            set
            {
                if(_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }

        public HomePageViewModel()
        {
            // 示例数据（可以替换为从数据库或服务加载）
            Notes = new ObservableCollection<Note>
            {
                new Note { Title = "笔记 1", Content = "这是第一条笔记的内容...", FolderId = 1 },
                new Note { Title = "笔记 2", Content = "这是第二条笔记的内容...", FolderId = 2 },
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
