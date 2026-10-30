using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Globalization;

public enum FilterMode { All, Active, Done }
public enum SortMode { ByDeadline, ByTitle }

public class TodoList
{
    private readonly List<TodoItem> _items = new();
    private readonly string _filename;

    private FilterMode _filter = FilterMode.All;
    private SortMode _sort = SortMode.ByDeadline;
    private string _search = "";

    public TodoList(string filename = "tasks.json")
    {
        _filename = filename;
        Load();
    }

    public TodoItem Add(string title, string deadline = "")
    {
        var item = new TodoItem(title, deadline);
        _items.Add(item);
        Save();
        return item;
    }

    public void CompleteItem(TodoItem item)
{
    int index = _items.IndexOf(item);
    if (index >= 0)
    {
        _items[index].Complete();
        Save();
    }
}

public void RemoveItem(TodoItem item)
{
    _items.Remove(item);
    Save();
}

public void RenameItem(TodoItem item, string newTitle)
{
    int index = _items.IndexOf(item);
    if (index >= 0)
    {
        _items[index].Rename(newTitle);
        Save();
    }
}

    public TodoItem Get(int number) => _items[number - 1];

    public IReadOnlyList<TodoItem> All() => _items;

    public int Count => _items.Count;

    public int DoneCount
    {
        get
        {
            int c = 0;
            foreach (var item in _items)
                if (item.IsDone) c++;
            return c;
        }
    }

    public void SetFilter(FilterMode mode) => _filter = mode;
    public void SetSort(SortMode mode) => _sort = mode;
    public void SetSearch(string query) => _search = query?.ToLower() ?? "";

    public List<TodoItem> Filtered()
{
    List<TodoItem> result = new List<TodoItem>();

    foreach (TodoItem item in _items)
    {
        bool add = true; 

        if (_filter == FilterMode.Active && item.IsDone)
            add = false; 
        else if (_filter == FilterMode.Done && !item.IsDone)
            add = false; 

        if (!string.IsNullOrEmpty(_search))
        {
            if (!item.Title.ToLower().Contains(_search))
                add = false; 
        }

        if (add)
            result.Add(item);
    }

    if (_sort == SortMode.ByDeadline)
    {
        result.Sort((a, b) => a.Deadline.CompareTo(b.Deadline));
    }
    else if (_sort == SortMode.ByTitle)
    {
        result.Sort((a, b) => a.Title.CompareTo(b.Title));
    }

    return result;
}
    public bool IsOverdue(TodoItem item)
    {
        if (string.IsNullOrEmpty(item.Deadline)) return false;
        if (item.IsDone) return false;
        if (DateTime.TryParseExact(item.Deadline, "dd.MM.yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return date < DateTime.Today;
        }
        return false;
    }

    public void Save()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(_items, options);
        File.WriteAllText(_filename, json);
    }

    public void Load()
    {
        if (!File.Exists(_filename)) return;
        string json = File.ReadAllText(_filename);
        var items = JsonSerializer.Deserialize<List<TodoItem>>(json);
        if (items != null)
            _items.AddRange(items);
    }
}
