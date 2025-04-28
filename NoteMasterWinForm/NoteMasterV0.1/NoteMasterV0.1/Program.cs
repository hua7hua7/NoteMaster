using Microsoft.EntityFrameworkCore;
using NoteMasterV0._1.Data;
using NoteMasterV0._1.Helpers;
using NoteMasterV0._1.Services;
using NoteMasterV0._1.Views;

namespace NoteMasterV0._1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 初始化日志
            Logger.Initialize();

            try
            {
                // 创建数据库上下文
                var context = new NoteDbContext();
                context.Database.EnsureCreated();

                // 创建服务
                var noteService = new NoteService(context);

                // 创建主窗体
                var mainForm = new MainForm(noteService);

                // 运行应用程序
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                Logger.LogError("应用程序启动失败", ex);
                MessageBox.Show($"应用程序启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}