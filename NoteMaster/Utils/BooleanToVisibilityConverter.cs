using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NoteMaster.Utils
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool boolValue = value is bool boolVal ? boolVal : false; // 默认值为 false
                bool inverse = parameter?.ToString() == "Inverse";

                if (inverse)
                {
                    boolValue = !boolValue;
                }

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                // 记录异常以便调试
                Console.WriteLine($"BooleanToVisibilityConverter 转换失败: {ex.Message}");
                return Visibility.Collapsed; // 默认隐藏
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}