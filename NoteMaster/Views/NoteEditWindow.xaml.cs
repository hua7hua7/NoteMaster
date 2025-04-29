using NoteMaster.Models;
using System.Windows;

namespace NoteMaster
{
    public partial class NoteEditWindow : Window
    {
        public NoteEditWindow(Note note)
        {
            InitializeComponent();
            DataContext = note;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}