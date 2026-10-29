using System;

public class TodoItem
{
    private string _title = "";
    private bool _done;
    private string _deadline = "";

    public TodoItem() { }

    public TodoItem(string title, string deadline = "")
    {
        Rename(title);
        _deadline = deadline;
        _done = false;
    }

    public string Title => _title;
    public bool IsDone => _done;
    public string Deadline => _deadline;

    public void Complete() => _done = true;

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название не может быть пустым");
        _title = title.Trim();
    }

    public void SetDeadline(string deadline) => _deadline = deadline;

    public override string ToString()
    {
        string prefix = _done ? "✔ " : "☐ ";
        string text = prefix + _title;
        if (!string.IsNullOrEmpty(_deadline))
            text += "    " + _deadline;
        return text;
    }
}
