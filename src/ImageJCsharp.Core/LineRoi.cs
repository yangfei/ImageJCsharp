namespace ImageJCsharp.Core;

public readonly struct LineRoi
{
    public LineRoi(int x1, int y1, int x2, int y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    public int X1 { get; }

    public int Y1 { get; }

    public int X2 { get; }

    public int Y2 { get; }
}
