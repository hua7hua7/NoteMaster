using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using NoteMaster.ViewModels;

namespace NoteMaster.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void rdHome_Click(object sender, RoutedEventArgs e)
        {
            PagesNavigation.Navigate(new System.Uri("/Views/Pages/HomePage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void rdTodos_Click(object sender, RoutedEventArgs e)
        {
            PagesNavigation.Navigate(new System.Uri("/Views/Pages/TodoPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void rdNotes_Click(object sender, RoutedEventArgs e)
        {
            PagesNavigation.Navigate(new System.Uri("/Views/Pages/NotesPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void rdPayment_Click(object sender, RoutedEventArgs e)
        {
            PagesNavigation.Navigate(new System.Uri("/Views/Pages/PaymentPage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}