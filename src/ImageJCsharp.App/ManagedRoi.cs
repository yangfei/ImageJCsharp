using System;
using ImageJCsharp.Core;

namespace ImageJCsharp.App;

public sealed class ManagedRoi
{
    private ManagedRoi(RoiShape shape, RectRoi? bounds, LineRoi? line)
    {
        Shape = shape;
        Bounds = bounds;
        Line = line;
    }

    public RoiShape Shape { get; }

    public RectRoi? Bounds { get; }

    public LineRoi? Line { get; }

    public static ManagedRoi Rectangle(RectRoi roi)
    {
        return new ManagedRoi(RoiShape.Rectangle, roi, null);
    }

    public static ManagedRoi Oval(RectRoi bounds)
    {
        return new ManagedRoi(RoiShape.Oval, bounds, null);
    }

    public static ManagedRoi FromLine(LineRoi line)
    {
        return new ManagedRoi(RoiShape.Line, null, line);
    }

    public override string ToString()
    {
        return Shape switch
        {
            RoiShape.Rectangle => FormatBounds("Rectangle", Bounds ?? throw new InvalidOperationException("Rectangle ROI has no bounds.")),
            RoiShape.Oval => FormatBounds("Oval", Bounds ?? throw new InvalidOperationException("Oval ROI has no bounds.")),
            RoiShape.Line => FormatLine(Line ?? throw new InvalidOperationException("Line ROI has no line.")),
            _ => Shape.ToString()
        };
    }

    private static string FormatBounds(string name, RectRoi roi)
    {
        return $"{name} {roi.X},{roi.Y} {roi.Width}x{roi.Height}";
    }

    private static string FormatLine(LineRoi line)
    {
        return $"Line {line.X1},{line.Y1} to {line.X2},{line.Y2}";
    }
}
