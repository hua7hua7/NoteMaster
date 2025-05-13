using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NoteMaster.ViewModels;

namespace NoteMaster.Views.Pages
{
    public partial class TodoPage : Page
    {
        public TodoPage()
        {
            try
            {
                InitializeComponent();
                DataContext = new TodoPageViewModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TodoPage 初始化异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    var viewModel = DataContext as TodoPageViewModel;
                    if (viewModel?.AddTodoCommand.CanExecute(null) == true)
                    {
                        viewModel.AddTodoCommand.Execute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TextBox_KeyDown 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            try
            {
                base.OnInitialized(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnInitialized 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            try
            {
                base.OnRender(drawingContext);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnRender 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}