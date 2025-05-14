using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NoteMaster.Views.Pages
{
    public partial class TodoPage : Page
    {
        public TodoPage()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox)
            {
                var viewModel = DataContext as NoteMaster.ViewModels.TodoPageViewModel;
                if (viewModel?.AddTodoCommand.CanExecute(null) == true)
                {
                    viewModel.AddTodoCommand.Execute(null);
                }
            }
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }
            return value;
        }
    }
}