using System;
using System.Drawing;

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
