using System;

namespace ImageJCsharp.Core;

public readonly struct OvalRoi
{
    public OvalRoi(int x, int y, int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public int X { get; }

    public int Y { get; }

    public int Width { get; }

    public int Height { get; }

    public int Right => X + Width;

    public int Bottom => Y + Height;

    public bool ContainsPixel(int x, int y)
    {
        var radiusX = (Width - 1) / 2d;
        var radiusY = (Height - 1) / 2d;
        var centerX = X + radiusX;
        var centerY = Y + radiusY;
        var normalizedX = radiusX == 0 ? 0 : (x - centerX) / radiusX;
        var normalizedY = radiusY == 0 ? 0 : (y - centerY) / radiusY;
        return (normalizedX * normalizedX) + (normalizedY * normalizedY) <= 1d;
    }
}
