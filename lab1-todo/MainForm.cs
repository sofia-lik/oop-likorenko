using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class MainForm : Form
{
    private readonly TodoList _list;

    private TextBox _titleBox;
    private TextBox _dateBox;
    private TextBox _searchBox;

    private Button _addButton;
    private Button _completeButton;
    private Button _renameButton;
    private Button _deleteButton;
    private Button _allButton;
    private Button _activeButton;
    private Button _doneButton;

    private ListBox _listBox;
    private ComboBox _sortCombo;

    private Label _statusLabel;

    public MainForm()
    {
        _list = new TodoList("tasks.json");
        InitializeComponent();
        RefreshList();
    }

    private void InitializeComponent()
    {
        this.Text = "Мои задачи";
        this.Size = new Size(600, 520);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(500, 400);

        _titleBox = new TextBox();
        _titleBox.Location = new Point(10, 10);
        _titleBox.Size = new Size(340, 25);
        _titleBox.PlaceholderText = "Название задачи...";

        _dateBox = new TextBox();
        _dateBox.Location = new Point(360, 10);
        _dateBox.Size = new Size(110, 25);
        _dateBox.PlaceholderText = "24.10.2026";

        _addButton = new Button();
        _addButton.Location = new Point(480, 10);
        _addButton.Size = new Size(100, 25);
        _addButton.Text = "Добавить";
        _addButton.Click += AddButton_Click;

        _searchBox = new TextBox();
        _searchBox.Location = new Point(10, 45);
        _searchBox.Size = new Size(570, 25);
        _searchBox.PlaceholderText = "🔍 Поиск...";
        _searchBox.TextChanged += SearchBox_TextChanged;

        _listBox = new ListBox();
        _listBox.Location = new Point(10, 80);
        _listBox.Size = new Size(570, 280);
        _listBox.Font = new Font("Segoe UI", 11);
        _listBox.DrawMode = DrawMode.OwnerDrawFixed;
        _listBox.ItemHeight = 30;
        _listBox.DrawItem += ListBox_DrawItem;

        _allButton = new Button { Text = "Все", Location = new Point(10, 370), Size = new Size(70, 30) };
        _allButton.Click += (s, e) => { _list.SetFilter(FilterMode.All); RefreshList(); };

        _activeButton = new Button { Text = "Активные", Location = new Point(90, 370), Size = new Size(90, 30) };
        _activeButton.Click += (s, e) => { _list.SetFilter(FilterMode.Active); RefreshList(); };

        _doneButton = new Button { Text = "Выполненные", Location = new Point(190, 370), Size = new Size(110, 30) };
        _doneButton.Click += (s, e) => { _list.SetFilter(FilterMode.Done); RefreshList(); };

        _sortCombo = new ComboBox();
        _sortCombo.Location = new Point(320, 370);
        _sortCombo.Size = new Size(150, 25);
        _sortCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _sortCombo.Items.AddRange(new object[] { "По сроку", "По названию" });
        _sortCombo.SelectedIndex = 0;
        _sortCombo.SelectedIndexChanged += (s, e) =>
        {
            _list.SetSort(_sortCombo.SelectedIndex == 0 ? SortMode.ByDeadline : SortMode.ByTitle);
            RefreshList();
        };

        _completeButton = new Button { Text = "✓ Выполнено", Location = new Point(10, 410), Size = new Size(130, 30) };
        _completeButton.Click += CompleteButton_Click;

        _renameButton = new Button { Text = "✎ Переименовать", Location = new Point(150, 410), Size = new Size(140, 30) };
        _renameButton.Click += RenameButton_Click;

        _deleteButton = new Button { Text = "🗑 Удалить", Location = new Point(300, 410), Size = new Size(110, 30) };
        _deleteButton.Click += DeleteButton_Click;

        _statusLabel = new Label();
        _statusLabel.Location = new Point(10, 450);
        _statusLabel.Size = new Size(570, 20);
        _statusLabel.Text = "";

        this.Controls.AddRange(new Control[] {
            _titleBox, _dateBox, _addButton,
            _searchBox,
            _listBox,
            _allButton, _activeButton, _doneButton, _sortCombo,
            _completeButton, _renameButton, _deleteButton,
            _statusLabel
        });
    }

    private void RefreshList()
    {
        _listBox.Items.Clear();
        foreach (var item in _list.Filtered())
            _listBox.Items.Add(item);

        _statusLabel.Text = $"{_list.Count} задач, выполнено {_list.DoneCount} · сохранено в tasks.json";
    }

    private void AddButton_Click(object sender, EventArgs e)
    {
        try
        {
            _list.Add(_titleBox.Text, _dateBox.Text);
            _titleBox.Clear();
            _dateBox.Clear();
            RefreshList();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void CompleteButton_Click(object sender, EventArgs e)
    {
        if (_listBox.SelectedIndex < 0)
        {
            MessageBox.Show("Выберите задачу в списке", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _list.Complete(_listBox.SelectedIndex);
        RefreshList();
    }

    private void RenameButton_Click(object sender, EventArgs e)
    {
        if (_listBox.SelectedIndex < 0)
        {
            MessageBox.Show("Выберите задачу в списке", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string currentTitle = _list.Get(_listBox.SelectedIndex + 1).Title;

        using (var form = new Form())
        {
            form.Text = "Переименовать";
            form.Size = new Size(350, 150);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;

            var textBox = new TextBox { Text = currentTitle, Location = new Point(10, 20), Width = 310 };
            var okBtn = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(130, 60), Width = 80 };
            var cancelBtn = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new Point(220, 60), Width = 80 };

            form.Controls.AddRange(new Control[] { textBox, okBtn, cancelBtn });
            form.AcceptButton = okBtn;
            form.CancelButton = cancelBtn;

            if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                try
                {
                    _list.Rename(_listBox.SelectedIndex, textBox.Text);
                    RefreshList();
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }

    private void DeleteButton_Click(object sender, EventArgs e)
    {
        if (_listBox.SelectedIndex < 0)
        {
            MessageBox.Show("Выберите задачу в списке", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _list.Remove(_listBox.SelectedIndex);
        RefreshList();
    }

    private void SearchBox_TextChanged(object sender, EventArgs e)
    {
        _list.SetSearch(_searchBox.Text);
        RefreshList();
    }

    private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var items = _list.Filtered();
        var item = (TodoItem)items[e.Index];

        e.DrawBackground();

        Color textColor = e.ForeColor;
        if (_list.IsOverdue(item))
            textColor = Color.Red;  
        else if (item.IsDone)
            textColor = Color.Gray;  

        using (var brush = new SolidBrush(textColor))
        {
            string text = item.ToString();
            e.Graphics.DrawString(text, e.Font, brush, e.Bounds.X + 4, e.Bounds.Y + 6);
        }

        e.DrawFocusRectangle();
    }
}
