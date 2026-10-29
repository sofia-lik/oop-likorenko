using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public void Remove(int index)
    {
        _items.RemoveAt(index);
        Save();
    }

    public void Complete(int index)
    {
        _items[index].Complete();
        Save();
    }

    public void Rename(int index, string newTitle)
    {
        _items[index].Rename(newTitle);
        Save();
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

    public IReadOnlyList<TodoItem> Filtered()
    {
        IEnumerable<TodoItem> result = _items;

        if (_filter == FilterMode.Active)
            result = result.Where(i => !i.IsDone);
        else if (_filter == FilterMode.Done)
            result = result.Where(i => i.IsDone);

        if (!string.IsNullOrEmpty(_search))
            result = result.Where(i => i.Title.ToLower().Contains(_search));

        if (_sort == SortMode.ByDeadline)
            result = result.OrderBy(i => i.Deadline);
        else if (_sort == SortMode.ByTitle)
            result = result.OrderBy(i => i.Title);

        return result.ToList();
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
