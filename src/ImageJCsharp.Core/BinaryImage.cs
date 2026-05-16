using System;

namespace ImageJCsharp.Core;

public sealed class BinaryImage
{
    private readonly bool[] _pixels;

    public BinaryImage(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        Width = width;
        Height = height;
        _pixels = new bool[checked(width * height)];
    }

    public int Width { get; }

    public int Height { get; }

    public bool this[int x, int y]
    {
        get => _pixels[GetIndex(x, y)];
        set => _pixels[GetIndex(x, y)] = value;
    }

    private int GetIndex(int x, int y)
    {
        if ((uint)x >= (uint)Width)
        {
            throw new ArgumentOutOfRangeException(nameof(x));
        }

        if ((uint)y >= (uint)Height)
        {
            throw new ArgumentOutOfRangeException(nameof(y));
        }

        return y * Width + x;
    }
}
