using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Model;

namespace ServerApp;

public class ClientHandler
{
    private readonly ChatServer _server;
    private readonly TcpClient _tcp;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly object _sendLock = new();

    public string? Nick { get; set; }

    public ClientHandler(ChatServer server, TcpClient tcp)
    {
        _server = server;
        _tcp = tcp;
        var stream = tcp.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
    }

    public void Run()
    {
        try
        {
            string? line;
            while ((line = _reader.ReadLine()) != null)   
            {
                var message = ChatMessage.FromLine(line);

                if (message.Type == "join")
                {
                    if (Nick != null) continue;       
                    var nick = message.Nick.Trim();
                    if (nick.Length == 0 || nick.Contains(','))
                    {
                        Send(new ChatMessage("error", "", "Недопустимый ник"));
                        break;
                    }
                    if (!_server.TryRegister(this, nick))
                    {
                        Send(new ChatMessage("error", "", $"Ник «{nick}» уже занят"));
                        break;
                    }
                    foreach (var old in _server.LoadHistory()) 
                        Send(old);
                    _server.Broadcast(new ChatMessage("system", "", $"{Nick} вошёл в чат"));
                    _server.BroadcastUsers();
                }
                else if (Nick == null)
                {
                    break;                             
                }
                else if (message.Type == "private")
                {
                    _server.SendPrivate(this, message.To, message.Text);
                }
                else
                {
                    var text = new ChatMessage("text", Nick, message.Text);
                    _server.SaveToHistory(text);             
                    _server.Broadcast(text);
                }
            }
        }
        catch (IOException) { }                               
        catch (JsonException) { }                             
        catch (FormatException) { }
        finally
        {
            _server.Remove(this);
            _tcp.Close();
            if (Nick != null)
            {
                Console.WriteLine($"{Nick} вышел");
                _server.Broadcast(new ChatMessage("system", "", $"{Nick} вышел"));
                _server.BroadcastUsers();
            }
        }
    }

    public void Send(ChatMessage message)
    {
        try
        {
            lock (_sendLock)                                 
                _writer.WriteLine(message.ToLine());
        }
        catch (IOException) { }                              
        catch (ObjectDisposedException) { }
    }
}
