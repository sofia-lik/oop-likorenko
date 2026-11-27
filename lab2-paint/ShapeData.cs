using System.Collections.Generic;

public record ShapeData(
    string Type,
    int X1, int Y1, int X2, int Y2,
    string Color, int Width,
    List<int[]>? Points = null
);
