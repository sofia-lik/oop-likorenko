using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public record ShapeData(
string Type,
int X1, int Y1, int X2, int Y2,
string Color, int Width,
List<int[]>? Points = null 
);

public abstract class Shape
{
    public int X1 { get; protected set; }
    public int Y1 { get; protected set; }
    public int X2 { get; private set; }
    public int Y2 { get; private set; }
    public string Color { get; }
    public int Width { get; }

    protected Shape(int x1, int y1, int x2, int y2, string color = "#000000", int width = 2)
    {
        X1 = x1; Y1 = y1; X2 = x2; Y2 = y2;
        Color = color;
        Width = width;
    }

    public virtual void MoveEnd(int x, int y) { X2 = x; Y2 = y; }

    public virtual void Move(int dx, int dy)
    {
        X1 += dx;
        Y1 += dy;
        X2 += dx;
        Y2 += dy;
    }

    public virtual (int X, int Y, int W, int H) Box() =>
    (Math.Min(X1, X2), Math.Min(Y1, Y2),
    Math.Abs(X2 - X1), Math.Abs(Y2 - Y1));

    public abstract void Draw(ICanvas canvas);

    public abstract bool Contains(Point point);

    public virtual ShapeData ToData() =>
    new(GetType().Name, X1, Y1, X2, Y2, Color, Width);

    public virtual void Restore(ShapeData data) { }

    protected double HitTolerance => Math.Max(5, Width / 2.0 + 2);

    protected static double DistancePointToLine(Point p, Point a, Point b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double lengthSquared = dx * dx + dy * dy;

        if (lengthSquared == 0)
            return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));

        double t = Math.Max(0, Math.Min(1,
        ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared));
        double projX = a.X + t * dx;
        double projY = a.Y + t * dy;

        return Math.Sqrt((p.X - projX) * (p.X - projX) +
        (p.Y - projY) * (p.Y - projY));
    }
}

public class LineShape : Shape
{
    public LineShape(int x1, int y1, int x2, int y2,
    string color = "#000000", int width = 2)
    : base(x1, y1, x2, y2, color, width) { }

    public override void Draw(ICanvas canvas) =>
    canvas.Line(X1, Y1, X2, Y2, Color, Width);

    public override bool Contains(Point point) =>
    DistancePointToLine(point, new Point(X1, Y1), new Point(X2, Y2)) <= HitTolerance;
}

public class RectShape : Shape
{
    public RectShape(int x1, int y1, int x2, int y2,
    string color = "#000000", int width = 2)
    : base(x1, y1, x2, y2, color, width) { }

    public override void Draw(ICanvas canvas)
    {
        var (x, y, w, h) = Box();
        canvas.Rect(x, y, w, h, Color, Width);
    }

    public override bool Contains(Point point)
    {
        var (x, y, w, h) = Box();
        return point.X >= x && point.X <= x + w &&
        point.Y >= y && point.Y <= y + h;
    }
}

public class EllipseShape : Shape
{
    public EllipseShape(int x1, int y1, int x2, int y2,
    string color = "#000000", int width = 2)
    : base(x1, y1, x2, y2, color, width) { }

    public override void Draw(ICanvas canvas)
    {
        var (x, y, w, h) = Box();
        canvas.Ellipse(x, y, w, h, Color, Width);
    }

    public override bool Contains(Point point)
    {
        var (x, y, w, h) = Box();
        float cx = x + w / 2f;
        float cy = y + h / 2f;
        float rx = Math.Max(w / 2f, 1);
        float ry = Math.Max(h / 2f, 1);

        float dx = point.X - cx;
        float dy = point.Y - cy;

        return (dx * dx) / (rx * rx) + (dy * dy) / (ry * ry) <= 1;
    }
}

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
