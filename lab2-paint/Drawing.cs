using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;

public class Drawing
{
    private readonly List<Shape> _shapes = new();

    public void Add(Shape shape) => _shapes.Add(shape);

    public void Remove(Shape shape) => _shapes.Remove(shape);

    public int IndexOf(Shape shape) => _shapes.IndexOf(shape);

    public void Insert(int index, Shape shape)
    {
        if (index < 0 || index > _shapes.Count) _shapes.Add(shape);
        else _shapes.Insert(index, shape);
    }

    public IEnumerable<Shape> GetAll() => _shapes.AsReadOnly();

    public void Undo()
    {
        if (_shapes.Count > 0)
            _shapes.RemoveAt(_shapes.Count - 1);
    }

    public void Clear() => _shapes.Clear();

    public void Draw(ICanvas canvas)
    {
        foreach (var shape in _shapes)
            shape.Draw(canvas);
    }

    public string ToJson() =>
    JsonSerializer.Serialize(
    _shapes.Select(s => s.ToData()),
    new JsonSerializerOptions { WriteIndented = true });

    public static Drawing FromJson(string json)
    {
        var data = JsonSerializer.Deserialize<List<ShapeData>>(json) ?? new List<ShapeData>();
        var drawing = new Drawing();
        foreach (var d in data)
            drawing.Add(ShapeFactory.FromData(d));
        return drawing;
    }

    public Shape? GetShapeAt(Point point)
    {
        for (int i = _shapes.Count - 1; i >= 0; i--)
        {
            if (_shapes[i].Contains(point))
                return _shapes[i];
        }
        return null;
    }
}
