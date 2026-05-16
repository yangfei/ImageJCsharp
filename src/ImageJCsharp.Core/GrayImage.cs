using System;

namespace ImageJCsharp.Core;

public sealed class GrayImage
{
    private readonly ushort[] _pixels;

    public GrayImage(int width, int height)
        : this(width, height, new ushort[CheckedLength(width, height)])
    {
    }

    private GrayImage(int width, int height, ushort[] pixels)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        if (pixels.Length != width * height)
        {
            throw new ArgumentException("Pixel buffer length must match width * height.", nameof(pixels));
        }

        Width = width;
        Height = height;
        _pixels = pixels;
    }

    public int Width { get; }

    public int Height { get; }

    public ushort this[int x, int y]
    {
        get => _pixels[GetIndex(x, y)];
        set => _pixels[GetIndex(x, y)] = value;
    }

    public static GrayImage FromPixels(int width, int height, ushort[] pixels)
    {
        if (pixels is null)
        {
            throw new ArgumentNullException(nameof(pixels));
        }

        var copy = new ushort[pixels.Length];
        Array.Copy(pixels, copy, pixels.Length);
        return new GrayImage(width, height, copy);
    }

    public ushort[] CopyPixels()
    {
        var copy = new ushort[_pixels.Length];
        Array.Copy(_pixels, copy, _pixels.Length);
        return copy;
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

    private static int CheckedLength(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        return checked(width * height);
    }
}
