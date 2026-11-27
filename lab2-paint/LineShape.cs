using System.Drawing;

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
