using System;
using System.Drawing;

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
    public IEnumerable<(int x, int y)> GetPoints() => _points.AsReadOnly();
}
