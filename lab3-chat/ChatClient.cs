using System.Net.Sockets;
using System.Text;

namespace Model;

public class ChatClient
{
    private readonly TcpClient _tcp = new();
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly object _sendLock = new();
    private readonly string _nick;

    public event Action<ChatMessage>? MessageReceived;

    public ChatClient(string host, int port, string nick)
    {
        _nick = nick;
        try
        {
            if (!_tcp.ConnectAsync(host, port).Wait(TimeSpan.FromSeconds(5)))
            {
                _tcp.Close();
                throw new IOException($"Сервер {host}:{port} не отвечает");
            }
        }
        catch (AggregateException ex)
        {
            _tcp.Close();
            throw new IOException($"Не удалось подключиться к {host}:{port}", ex.InnerException);
        }

        var stream = _tcp.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

        Send(new ChatMessage("join", nick, ""));
    }

    public void Start() => new Thread(ReadLoop) { IsBackground = true }.Start();

    private void ReadLoop()
    {
        try
        {
            string? line;
            while ((line = _reader.ReadLine()) != null)
            {
                try { MessageReceived?.Invoke(ChatMessage.FromLine(line)); }
                catch (System.Text.Json.JsonException) { }
                catch (FormatException) { }
            }
        }
        catch (IOException) { }
        catch (ObjectDisposedException) { }

        MessageReceived?.Invoke(new ChatMessage("system", "", "Соединение с сервером потеряно"));
    }

    private void Send(ChatMessage message)
    {
        lock (_sendLock)
            _writer.WriteLine(message.ToLine());
    }

    public void Say(string text) => Send(new ChatMessage("text", _nick, text));

    public void SayPrivate(string to, string text) =>
        Send(new ChatMessage("private", _nick, text) { To = to });

    public void Close() => _tcp.Close();
}
