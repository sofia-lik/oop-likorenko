public interface ICanvas
{
    void Line(int x1, int y1, int x2, int y2, string color, int width);
    void Rect(int x, int y, int w, int h, string color, int width);
    void Ellipse(int x, int y, int w, int h, string color, int width);
}
