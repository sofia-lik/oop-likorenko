using Model;

namespace WinFormsApp;

public class MainForm : Form
{
    private const string All = "Всем";
    private readonly ChatClient _client;
    private readonly string _nick;
    private readonly RichTextBox _feed = new() { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.White };
    private readonly ListBox _users = new() { Dock = DockStyle.Fill };
    private readonly Label _usersCaption = new() { Text = "Онлайн (0)", Dock = DockStyle.Top, Height = 22 };
    private readonly TextBox _input = new() { Dock = DockStyle.Fill };
    private readonly Button _send = new() { Text = "Отправить", Dock = DockStyle.Right, Width = 100 };

    private void UpdateRecipientHint()
    {
        var isPrivate = _users.SelectedItem is string who && who != All && who != _nick;
        _input.PlaceholderText = isPrivate
            ? $"Личное сообщение для {_users.SelectedItem}"
            : "Введите сообщение";
    }
    public MainForm(ChatClient client, string nick)
    {
        _client = client;
        _nick = nick;

        Text = $"Чат — {nick}";
        ClientSize = new Size(760, 500);
        MinimumSize = new Size(500, 350);
        StartPosition = FormStartPosition.CenterScreen;

        _usersCaption.Dock = DockStyle.Fill;
        _usersCaption.TextAlign = ContentAlignment.BottomLeft;
        _send.Dock = DockStyle.Fill;
        _send.AutoSize = true;

        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, RowCount = 1 };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bottom.Controls.Add(_input, 0, 0);
        bottom.Controls.Add(_send, 1, 0);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_feed, 0, 0);
        layout.SetRowSpan(_feed, 2);
        layout.Controls.Add(_usersCaption, 1, 0);
        layout.Controls.Add(_users, 1, 1);
        layout.Controls.Add(bottom, 0, 2);
        layout.SetColumnSpan(bottom, 2);
        Controls.Add(layout);

        _send.Click += (_, _) => SendText();
        AcceptButton = _send;
        _users.Items.Add(All);
        _users.SelectedIndex = 0;
        _users.SelectedIndexChanged += (_, _) => UpdateRecipientHint();
        UpdateRecipientHint();
        FormClosing += (_, _) => _client.Close();

        _client.MessageReceived += m =>
        {
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(() => Handle(m)); }
            catch (InvalidOperationException) { }          
        };
        _client.Start();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var area = Screen.FromControl(this).WorkingArea;
        Size = new Size(Math.Min(Width, area.Width * 3 / 4), Math.Min(Height, area.Height * 3 / 4));
        CenterToScreen();
        _input.Focus();
    }

    private void Handle(ChatMessage m)
    {
        switch (m.Type)
        {
            case "users":
                var nicks = m.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var selected = _users.SelectedItem as string ?? All;
                _users.Items.Clear();
                _users.Items.Add(All);
                _users.Items.AddRange(nicks);
                _users.SelectedItem = _users.Items.Contains(selected) ? selected : All;
                _usersCaption.Text = $"Онлайн ({nicks.Length})";
                break;
            case "system":
                Show($"{m.Time:HH:mm} {m}", Color.Gray, italic: true);
                break;
            case "error":
                Show($"{m.Time:HH:mm} {m}", Color.Firebrick, italic: false);
                break;
            case "private":
                var line = m.Nick == _nick
                    ? $"вы → {m.To}: {m.Text}"
                    : $"{m.Nick} → вам лично: {m.Text}";
                Show($"{m.Time:HH:mm} {line}", Color.DarkViolet, italic: false);
                break;
            default:
                Show($"{m.Time:HH:mm} {m}", Color.Black, italic: false);
                break;
        }
    }

    private void Show(string text, Color color, bool italic)
    {
        _feed.SelectionStart = _feed.TextLength;
        _feed.SelectionColor = color;
        _feed.SelectionFont = new Font(_feed.Font, italic ? FontStyle.Italic : FontStyle.Regular);
        _feed.AppendText(text + Environment.NewLine);
        _feed.ScrollToCaret();
    }

    private void SendText()
    {
        var text = _input.Text.Trim();
        if (text.Length == 0) return;

        try
        {
            if (_users.SelectedItem is string who && who != All && who != _nick)
                _client.SayPrivate(who, text);
            else
                _client.Say(text);
            _input.Clear();
        }
        catch (IOException)
        {
            Show("Нет соединения с сервером", Color.Firebrick, italic: false);
        }
        _input.Focus();
    }
}
