using System.Net;
using System.Net.Sockets;
using Model;
using System.Text.Json;

namespace ServerApp;

public class ChatServer
{
    private readonly List<ClientHandler> _clients = new();
    private readonly object _historyLock = new();
    private readonly string _historyPath = Path.Combine(AppContext.BaseDirectory, "history.jsonl");
    private readonly object _lock = new();      

    public void Serve(int port)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Чат-сервер ждёт на порту {port}");
        while (true)
        {
            var tcp = listener.AcceptTcpClient();         
            Console.WriteLine($"Подключился {tcp.Client.RemoteEndPoint}");
            var handler = new ClientHandler(this, tcp);
            lock (_lock)
                _clients.Add(handler);
            new Thread(handler.Run) { IsBackground = true }.Start();
        }
    }

    public void Remove(ClientHandler client)
    {
        lock (_lock)
            _clients.Remove(client);
    }

    public void SaveToHistory(ChatMessage message)
    {
        lock (_historyLock)
        {
            try { File.AppendAllText(_historyPath, message.ToLine() + Environment.NewLine); }
            catch (IOException) { }           
        }
    }

    public List<ChatMessage> LoadHistory(int count = 20)
    {
        var result = new List<ChatMessage>();
        lock (_historyLock)
        {
            if (!File.Exists(_historyPath)) return result;
            try
            {
                foreach (var line in File.ReadAllLines(_historyPath).TakeLast(count))
                {
                    try { result.Add(ChatMessage.FromLine(line)); }
                    catch (JsonException) { }     
                    catch (FormatException) { }
                }
            }
            catch (IOException) { }
        }
        return result;
    }

    public bool TryRegister(ClientHandler client, string nick)
    {
        lock (_lock)
        {
            if (_clients.Any(c => c.Nick != null &&
                                  string.Equals(c.Nick, nick, StringComparison.OrdinalIgnoreCase)))
                return false;
            client.Nick = nick;
            return true;
        }
    }

    public void Broadcast(ChatMessage message)
    {
        List<ClientHandler> copy;
        lock (_lock)                 
            copy = _clients.Where(c => c.Nick != null).ToList();
        foreach (var client in copy)                   
            client.Send(message);
    }

    public void BroadcastUsers()
    {
        string nicks;
        lock (_lock)
            nicks = string.Join(",", _clients.Where(c => c.Nick != null).Select(c => c.Nick));
        Broadcast(new ChatMessage("users", "", nicks));
    }

    public void SendPrivate(ClientHandler sender, string to, string text)
    {
        ClientHandler? target;
        lock (_lock)
            target = _clients.FirstOrDefault(c => c.Nick != null &&
                string.Equals(c.Nick, to, StringComparison.OrdinalIgnoreCase));

        if (target == null)
        {
            sender.Send(new ChatMessage("error", "", $"Пользователь «{to}» не в сети"));
            return;
        }
        var message = new ChatMessage("private", sender.Nick!, text) { To = target.Nick! };
        target.Send(message);
        if (target != sender)
            sender.Send(message);  
    }
}
