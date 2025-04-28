using NoteMasterV0._1.Models;
using NoteMasterV0._1.Services;

namespace NoteMasterV0._1.Views
{
    public partial class NoteForm : Form
    {
        private readonly INoteService _noteService;
        private readonly Note _note;
        private System.Windows.Forms.Timer _autoSaveTimer = null!;
        private TextBox _titleTextBox = null!;
        private TextBox _contentTextBox = null!;
        private TextBox _tagsTextBox = null!;

        public NoteForm(INoteService noteService, Note? note = null)
        {
            InitializeComponent();
            _noteService = noteService;
            _note = note ?? new Note();
            InitializeControls();
            InitializeAutoSave();
        }

        private void InitializeControls()
        {
            this.Text = _note.Id == 0 ? "新建便签" : "编辑便签";
            this.Size = new Size(600, 400);

            // 创建标题输入框
            var titleLabel = new Label { Text = "标题:", Location = new Point(10, 10) };
            _titleTextBox = new TextBox
            {
                Location = new Point(60, 10),
                Width = 500,
                Text = _note.Title
            };
            _titleTextBox.TextChanged += (s, e) => _note.Title = _titleTextBox.Text;

            // 创建内容输入框
            var contentLabel = new Label { Text = "内容:", Location = new Point(10, 40) };
            _contentTextBox = new TextBox
            {
                Location = new Point(60, 40),
                Width = 500,
                Height = 200,
                Multiline = true,
                Text = _note.Content
            };
            _contentTextBox.TextChanged += (s, e) => _note.Content = _contentTextBox.Text;

            // 创建标签输入框
            var tagsLabel = new Label { Text = "标签:", Location = new Point(10, 250) };
            _tagsTextBox = new TextBox
            {
                Location = new Point(60, 250),
                Width = 500,
                Text = string.Join(", ", _note.Tags)
            };
            _tagsTextBox.TextChanged += (s, e) =>
            {
                _note.Tags = _tagsTextBox.Text
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToList();
            };

            // 创建按钮
            var saveButton = new Button
            {
                Text = "保存",
                Location = new Point(400, 300),
                DialogResult = DialogResult.OK
            };
            saveButton.Click += SaveButton_Click;

            var cancelButton = new Button
            {
                Text = "取消",
                Location = new Point(500, 300),
                DialogResult = DialogResult.Cancel
            };

            // 添加控件
            this.Controls.AddRange(new Control[]
            {
                titleLabel, _titleTextBox,
                contentLabel, _contentTextBox,
                tagsLabel, _tagsTextBox,
                saveButton, cancelButton
            });
        }

        private void InitializeAutoSave()
        {
            _autoSaveTimer = new System.Windows.Forms.Timer();
            _autoSaveTimer.Interval = 30000; // 30秒
            _autoSaveTimer.Tick += AutoSaveTimer_Tick;
            _autoSaveTimer.Start();
        }

        private async void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_note.Title) || !string.IsNullOrWhiteSpace(_note.Content))
            {
                try
                {
                    if (_note.Id == 0)
                    {
                        await _noteService.CreateNoteAsync(_note);
                    }
                    else
                    {
                        await _noteService.UpdateNoteAsync(_note);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"自动保存失败: {ex.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_note.Id == 0)
                {
                    await _noteService.CreateNoteAsync(_note);
                }
                else
                {
                    await _noteService.UpdateNoteAsync(_note);
                }
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _autoSaveTimer.Stop();
            base.OnFormClosing(e);
        }
    }
} 