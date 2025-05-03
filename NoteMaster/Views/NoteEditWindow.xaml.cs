using System.Windows;
using NoteMaster.Models;

namespace NoteMaster.Views
{
    public partial class NoteEditWindow : Window
    {
        private readonly Note _note;

        public NoteEditWindow(Note note)
        {
            InitializeComponent();
            _note = note;
            DataContext = _note;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_note.Title))
            {
                MessageBox.Show("标题不能为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _note.UpdatedAt = System.DateTime.Now;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}