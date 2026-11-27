using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class PencilShape : Shape
{
    private readonly List<(int x, int y)> _points = new();

    public PencilShape(int x1, int y1,
        string color = "#000000", int width = 2)
        : base(x1, y1, x1, y1, color, width)
    {
        _points.Add((x1, y1));
    }

    public override void MoveEnd(int x, int y)
    {
        base.MoveEnd(x, y);
        _points.Add((x, y));
    }

    public override void Move(int dx, int dy)
    {
        base.Move(dx, dy);
        for (int i = 0; i < _points.Count; i++)
            _points[i] = (_points[i].x + dx, _points[i].y + dy);
    }

    public override (int X, int Y, int W, int H) Box()
    {
        int minX = _points.Min(p => p.x);
        int minY = _points.Min(p => p.y);
        int maxX = _points.Max(p => p.x);
        int maxY = _points.Max(p => p.y);
        return (minX, minY, maxX - minX, maxY - minY);
    }

    public override void Draw(ICanvas canvas)
    {
        for (int i = 1; i < _points.Count; i++)
            canvas.Line(
                _points[i - 1].x, _points[i - 1].y,
                _points[i].x, _points[i].y,
                Color, Width);
    }

    public override bool Contains(Point point)
    {
        for (int i = 1; i < _points.Count; i++)
        {
            if (DistancePointToLine(point,
                new Point(_points[i - 1].x, _points[i - 1].y),
                new Point(_points[i].x, _points[i].y)) <= HitTolerance)
                return true;
        }
        return false;
    }

    public override ShapeData ToData() =>
        base.ToData() with { Points = _points.Select(p => new[] { p.x, p.y }).ToList() };

    public override void Restore(ShapeData data)
    {
        if (data.Points == null || data.Points.Count == 0) return;

        _points.Clear();
        foreach (var p in data.Points)
            if (p.Length >= 2) _points.Add((p[0], p[1]));

        if (_points.Count == 0)
        {
            _points.Add((X1, Y1));
            return;
        }

        var last = _points[^1];
        base.MoveEnd(last.x, last.y);
    }

    public IEnumerable<(int x, int y)> GetPoints() => _points.AsReadOnly();
}
