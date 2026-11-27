using System.Drawing;

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
