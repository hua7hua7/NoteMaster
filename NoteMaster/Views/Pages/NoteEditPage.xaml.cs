using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NoteMaster.ViewModels;
using NoteMaster.Models;

namespace NoteMaster.Views.Pages
{
    /// <summary>
    /// NoteEditPage.xaml 的交互逻辑
    /// </summary>
    public partial class NoteEditPage : Page
    {
        private NoteEditPageViewModel _viewModel;
        public NoteEditPage()
        {
            InitializeComponent();
            _viewModel = new NoteEditPageViewModel();
            this.DataContext = _viewModel;
            _viewModel.NoteSaved += OnNoteSaved;
        }

        public NoteEditPage(Note note)
        {
            InitializeComponent();
            _viewModel = new NoteEditPageViewModel(note);
            this.DataContext = _viewModel;
            _viewModel.NoteSaved += OnNoteSaved;
            _viewModel.NoteDeleted += OnNoteDeleted;
        }

        private void OnNoteSaved(object? sender, EventArgs e)
        {
            // 跳转到HomePage
            NavigationService?.Navigate(new HomePage());
        }

        private void OnNoteDeleted(object? sender, EventArgs e)
        {
            // 删除后跳转到HomePage
            NavigationService?.Navigate(new HomePage());
        }

        //用于进行控制占位符的函数
        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TitlePlaceholder.Visibility = string.IsNullOrWhiteSpace(TitleTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ContentPlaceholder.Visibility = string.IsNullOrWhiteSpace(ContentTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        //返回按钮点击事件
        private void rd_HomePage(object? sender,EventArgs e)
        {
            //跳转回HomePage
            NavigationService?.Navigate(new HomePage());
        }
    }
}
