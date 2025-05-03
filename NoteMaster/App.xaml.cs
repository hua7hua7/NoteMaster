using System;
using System.Windows;
using NoteMaster.Views;
using NoteMaster.ViewModels;

namespace NoteMaster
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {

                base.OnStartup(e);
                
                // 创建主窗口
                //dev_A分支为：DataContext = new MainViewModel()
                var viewModel = new MainViewModel();
                var mainWindow = new MainWindow(viewModel);
                
                // 设置为主窗口
                Current.MainWindow = mainWindow;
                
                // 显示窗口
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"应用程序启动时发生错误：\n{ex.Message}\n\n堆栈跟踪：\n{ex.StackTrace}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                base.OnExit(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"应用程序退出时发生错误：\n{ex.Message}\n\n堆栈跟踪：\n{ex.StackTrace}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

        }
    }
}