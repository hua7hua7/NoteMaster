using NoteMaster.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NoteMaster.Utils
{
    public class FolderNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value 是 FolderId，parameter 是 Folders 集合
            int? folderId = value as int?;
            var folders = parameter as ObservableCollection<Folder>;
            if (folderId == null || folders == null)
                return "未分类";
            var folder = folders.FirstOrDefault(f => f.Id == folderId);
            return folder != null ? folder.Name : "未分类";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
}

