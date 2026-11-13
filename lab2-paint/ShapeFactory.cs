using System;
using System.Collections.Generic;
using System.IO;

public static class ShapeFactory
{
    public static readonly Dictionary<string,
    Func<int, int, int, int, string, int, Shape>> Kinds = new()
    {
        ["LineShape"] = (x1, y1, x2, y2, c, w) => new LineShape(x1, y1, x2, y2, c, w),
        ["RectShape"] = (x1, y1, x2, y2, c, w) => new RectShape(x1, y1, x2, y2, c, w),
        ["EllipseShape"] = (x1, y1, x2, y2, c, w) => new EllipseShape(x1, y1, x2, y2, c, w),
        ["PencilShape"] = (x1, y1, x2, y2, c, w) => new PencilShape(x1, y1, c, w),
    };

    public static Shape Create(string kind, int x, int y, string color, int width) =>
    Kinds[kind](x, y, x, y, color, width);

    public static Shape FromData(ShapeData d)
    {
        if (!Kinds.TryGetValue(d.Type, out var make))
            throw new InvalidDataException($"Неизвестный тип фигуры: {d.Type}");

        var shape = make(d.X1, d.Y1, d.X2, d.Y2, d.Color, d.Width);
        shape.Restore(d);
        return shape;
    }
}
