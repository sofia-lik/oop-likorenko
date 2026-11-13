using System.Drawing;
using System.Drawing.Drawing2D;

public class GraphicsCanvas : ICanvas
{
    private readonly Graphics _g;

    public GraphicsCanvas(Graphics g) => _g = g;

    private static Pen MakePen(string color, int width)
    {
        var pen = new Pen(ColorTranslator.FromHtml(color), width)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        return pen;
    }

    public void Line(int x1, int y1, int x2, int y2, string color, int width)
    {
        using var pen = MakePen(color, width);
        _g.DrawLine(pen, x1, y1, x2, y2);
    }

    public void Rect(int x, int y, int w, int h, string color, int width)
    {
        using var pen = MakePen(color, width);
        _g.DrawRectangle(pen, x, y, w, h);
    }

    public void Ellipse(int x, int y, int w, int h, string color, int width)
    {
        using var pen = MakePen(color, width);
        _g.DrawEllipse(pen, x, y, w, h);
    }
}
