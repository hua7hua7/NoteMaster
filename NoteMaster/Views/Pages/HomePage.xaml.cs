using NoteMaster.Models;
using NoteMaster.ViewModels;
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

namespace NoteMaster.Views.Pages
{
    /// <summary>
    /// Lógica de interacción para HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private HomePageViewModel _viewModel;

        public HomePage()
        {
            InitializeComponent();
            _viewModel = new HomePageViewModel();
            this.DataContext = _viewModel;
        }

        private void NoteCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Note selectedNote)
            {
                // 跳转到NoteEditPage进行编辑
                NavigationService?.Navigate(new NoteEditPage(selectedNote));
            }
        }
        private void rd_addNote(object sender, RoutedEventArgs e)
        {
            //跳转到新建清单页面
            NavigationService.Navigate(new Uri("/Views/Pages/NoteEditPage.xaml", UriKind.Relative));
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TagComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TagPlaceholder.Visibility = TagComboBox.SelectedItem == null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
