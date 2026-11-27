using Model;

namespace WinFormsApp;

public class LoginForm : Form
{
    private readonly TextBox _host = new() { Text = "127.0.0.1" };
    private readonly TextBox _port = new() { Text = "5000" };
    private readonly TextBox _nick = new();
    private readonly Button _login = new() { Text = "Войти" };

    public ChatClient? Client { get; private set; }
    public string Nick { get; private set; } = "";

    public LoginForm()
    {
        Text = "Вход в чат";
        ClientSize = new Size(300, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        AddRow("Сервер", _host, 15);
        AddRow("Порт", _port, 70);
        AddRow("Ник", _nick, 125);

        _login.SetBounds(195, 185, 90, 30);
        _login.Click += Login_Click;
        Controls.Add(_login);
        AcceptButton = _login;
    }

    private void AddRow(string caption, TextBox box, int y)
    {
        Controls.Add(new Label { Text = caption, Left = 15, Top = y, AutoSize = true });
        box.SetBounds(15, y + 20, 270, 25);
        Controls.Add(box);
    }

    private void Login_Click(object? sender, EventArgs e)
    {
        var nick = _nick.Text.Trim();
        if (nick.Length == 0 || nick.Contains(','))
        {
            MessageBox.Show("Введите ник (без запятых)");
            return;
        }
        if (!int.TryParse(_port.Text, out var port) || port is < 1 or > 65535)
        {
            MessageBox.Show("Порт: число от 1 до 65535");
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;
            Client = new ChatClient(_host.Text.Trim(), port, nick);
            Nick = nick;
            DialogResult = DialogResult.OK;
        }
        catch (IOException ex)                          
        {
            MessageBox.Show(ex.Message, "Ошибка подключения");
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }
}
