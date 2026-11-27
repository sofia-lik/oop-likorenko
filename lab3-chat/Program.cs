using ServerApp;

int port = 5000;
foreach (var arg in args)
{
    if (int.TryParse(arg, out var p)) port = p;
}

new ChatServer().Serve(port);
