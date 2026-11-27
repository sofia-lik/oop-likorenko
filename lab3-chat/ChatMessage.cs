using System.Text.Json;

namespace Model;
public record ChatMessage(string Type, string Nick, string Text)
{
    public string To { get; init; } = "";
    public DateTime Time { get; init; } = DateTime.Now;

    public string ToLine() => JsonSerializer.Serialize(this);

    public static ChatMessage FromLine(string line) =>
        JsonSerializer.Deserialize<ChatMessage>(line)
        ?? throw new FormatException("Пустое сообщение");

    public override string ToString() => Type switch
    {
        "system" => $"* {Text}",
        "error" => $"! {Text}",
        "private" => $"{Nick} → {To}: {Text}",
        _ => $"{Nick}: {Text}"
    };
}
