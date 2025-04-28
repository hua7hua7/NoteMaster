using NoteMasterV0._1.Models;
using NoteMasterV0._1.Services;

namespace NoteMasterV0._1.Views;

public partial class MainForm : Form
{
    private readonly INoteService _noteService;
    private List<Note> _notes = [];
    private ListView _noteList = null!;
    private ToolStripTextBox _searchBox = null!;
    private TextBox _titleTextBox = null!;
    private TextBox _contentTextBox = null!;
    private TextBox _tagsTextBox = null!;
    private ToolStripButton _saveButton = null!;
    private ToolStripButton _newButton = null!;
    private ToolStripButton _deleteButton = null!;
    private Note? _currentNote;
    private System.Windows.Forms.Timer _autoSaveTimer = null!;
    private SplitContainer _splitContainer = null!;
    private Panel _editPanel = null!;

    public MainForm(INoteService noteService)
    {
        InitializeComponent();
        _noteService = noteService;
        InitializeControls();
        InitializeAutoSave();
        _ = LoadNotesAsync();
    }

    private void InitializeControls()
    {
        Text = "NoteMaster";
        Size = new Size(1200, 800);

        // 创建工具栏
        var toolStrip = new ToolStrip();
        _newButton = new ToolStripButton("新建便签");
        _searchBox = new ToolStripTextBox();
        var searchButton = new ToolStripButton("搜索");
        _saveButton = new ToolStripButton("保存");
        _deleteButton = new ToolStripButton("删除");

        _newButton.Click += NewNoteButton_Click;
        searchButton.Click += (s, e) => _ = SearchNotesAsync(_searchBox.Text);
        _saveButton.Click += SaveButton_Click;
        _deleteButton.Click += DeleteButton_Click;

        toolStrip.Items.AddRange(new ToolStripItem[] 
        { 
            _newButton, 
            _searchBox, 
            searchButton,
            new ToolStripSeparator(),
            _saveButton,
            _deleteButton
        });
        Controls.Add(toolStrip);

        // 创建分割面板（上下结构，Panel1为列表，Panel2为编辑区）
        _splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal, // 上下结构
            SplitterDistance = 20, // 上半部分为列表，下半部分更高
            MinimumSize = new Size(400, 400)
        };

        // 创建便签列表（上半部分）
        _noteList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true
        };
        _noteList.Columns.Add("标题", 300);
        _noteList.Columns.Add("创建时间", 200);
        _noteList.Columns.Add("标签", 200);

        _noteList.SelectedIndexChanged += NoteList_SelectedIndexChanged;
        _splitContainer.Panel1.Controls.Add(_noteList);

        // 创建编辑面板（下半部分）
        _editPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.WhiteSmoke };
        
        int marginLeft = 40;
        int labelWidth = 60;
        int inputWidth = 1000;
        int spacingY = 30;
        int startY = 30;

        // 标题输入框
        var titleLabel = new Label { Text = "标题:", Location = new Point(marginLeft, startY), AutoSize = true };
        _titleTextBox = new TextBox
        {
            Location = new Point(marginLeft + labelWidth, startY - 5),
            Width = inputWidth,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Text = string.Empty
        };
        _titleTextBox.TextChanged += (s, e) => UpdateCurrentNote();

        // 内容输入框
        var contentLabel = new Label { Text = "内容:", Location = new Point(marginLeft, startY + spacingY), AutoSize = true };
        _contentTextBox = new TextBox
        {
            Location = new Point(marginLeft + labelWidth, startY + spacingY - 5),
            Width = inputWidth,
            Height = 350,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Text = string.Empty
        };
        _contentTextBox.TextChanged += (s, e) => UpdateCurrentNote();

        // 标签输入框（更靠下）
        int tagsY = startY + spacingY + 370;
        var tagsLabel = new Label { Text = "标签:", Location = new Point(marginLeft, tagsY), AutoSize = true };
        _tagsTextBox = new TextBox
        {
            Location = new Point(marginLeft + labelWidth, tagsY - 5),
            Width = inputWidth,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Text = string.Empty
        };
        _tagsTextBox.TextChanged += (s, e) => UpdateCurrentNote();

        // 添加控件到编辑面板
        _editPanel.Controls.AddRange(new Control[]
        {
            titleLabel, _titleTextBox,
            contentLabel, _contentTextBox,
            tagsLabel, _tagsTextBox
        });

        _splitContainer.Panel2.Controls.Add(_editPanel);
        Controls.Add(_splitContainer);
    }

    private void InitializeAutoSave()
    {
        _autoSaveTimer = new System.Windows.Forms.Timer();
        _autoSaveTimer.Interval = 30000; // 30秒
        _autoSaveTimer.Tick += AutoSaveTimer_Tick;
        _autoSaveTimer.Start();
    }

    private void UpdateCurrentNote()
    {
        if (_currentNote != null)
        {
            _currentNote.Title = _titleTextBox.Text;
            _currentNote.Content = _contentTextBox.Text;
            _currentNote.Tags = _tagsTextBox.Text
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToList();
        }
    }

    private async void AutoSaveTimer_Tick(object sender, EventArgs e)
    {
        if (_currentNote != null && (!string.IsNullOrWhiteSpace(_currentNote.Title) || !string.IsNullOrWhiteSpace(_currentNote.Content)))
        {
            try
            {
                if (_currentNote.Id == 0)
                {
                    await _noteService.CreateNoteAsync(_currentNote);
                }
                else
                {
                    await _noteService.UpdateNoteAsync(_currentNote);
                }
                await LoadNotesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"自动保存失败: {ex.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private async Task LoadNotesAsync()
    {
        try
        {
            _notes = await _noteService.GetAllNotesAsync();
            RefreshNoteList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载便签失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshNoteList()
    {
        _noteList.Items.Clear();

        foreach (var note in _notes)
        {
            var item = new ListViewItem(note.Title);
            item.SubItems.Add(note.CreatedAt.ToString());
            item.SubItems.Add(string.Join(", ", note.Tags));
            item.Tag = note;
            _noteList.Items.Add(item);
        }
    }

    private void NewNoteButton_Click(object sender, EventArgs e)
    {
        _currentNote = new Note();
        _titleTextBox.Text = string.Empty;
        _contentTextBox.Text = string.Empty;
        _tagsTextBox.Text = string.Empty;
        _noteList.SelectedItems.Clear();
    }

    private void NoteList_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_noteList.SelectedItems.Count > 0)
        {
            _currentNote = _noteList.SelectedItems[0].Tag as Note;
            if (_currentNote != null)
            {
                _titleTextBox.Text = _currentNote.Title;
                _contentTextBox.Text = _currentNote.Content;
                _tagsTextBox.Text = string.Join(", ", _currentNote.Tags);
            }
        }
    }

    private async void SaveButton_Click(object sender, EventArgs e)
    {
        if (_currentNote == null) return;

        try
        {
            UpdateCurrentNote();
            if (_currentNote.Id == 0)
            {
                await _noteService.CreateNoteAsync(_currentNote);
            }
            else
            {
                await _noteService.UpdateNoteAsync(_currentNote);
            }
            await LoadNotesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void DeleteButton_Click(object sender, EventArgs e)
    {
        if (_currentNote == null || _currentNote.Id == 0) return;

        if (MessageBox.Show("确定要删除这个便签吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            try
            {
                await _noteService.DeleteNoteAsync(_currentNote.Id);
                _currentNote = null;
                _titleTextBox.Text = string.Empty;
                _contentTextBox.Text = string.Empty;
                _tagsTextBox.Text = string.Empty;
                await LoadNotesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async Task SearchNotesAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            await LoadNotesAsync();
            return;
        }

        try
        {
            _notes = await _noteService.SearchNotesAsync(keyword);
            RefreshNoteList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"搜索便签失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _autoSaveTimer.Stop();
        base.OnFormClosing(e);
    }
} 