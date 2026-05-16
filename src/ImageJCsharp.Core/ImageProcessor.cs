using System;

namespace ImageJCsharp.Core;

public static class ImageProcessor
{
    public static BinaryImage Threshold(GrayImage image, ushort minimum, ushort maximum)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (minimum > maximum)
        {
            throw new ArgumentException("Minimum threshold must be less than or equal to maximum threshold.");
        }

        var result = new BinaryImage(image.Width, image.Height);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var value = image[x, y];
                result[x, y] = value >= minimum && value <= maximum;
            }
        }

        return result;
    }

    public static GrayImage Invert(GrayImage image, ushort maximum)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var result = new GrayImage(image.Width, image.Height);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                result[x, y] = (ushort)Math.Max(0, maximum - image[x, y]);
            }
        }

        return result;
    }

    public static BinaryImage Erode(BinaryImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var result = new BinaryImage(image.Width, image.Height);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                result[x, y] = AllNeighborsSet(image, x, y);
            }
        }

        return result;
    }

    public static BinaryImage Dilate(BinaryImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var result = new BinaryImage(image.Width, image.Height);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                result[x, y] = AnyNeighborSet(image, x, y);
            }
        }

        return result;
    }

    public static BinaryImage Open(BinaryImage image)
    {
        return Dilate(Erode(image));
    }

    public static BinaryImage Close(BinaryImage image)
    {
        return Erode(Dilate(image));
    }

    public static GrayImage SobelEdges(GrayImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var result = new GrayImage(image.Width, image.Height);
        for (var y = 1; y < image.Height - 1; y++)
        {
            for (var x = 1; x < image.Width - 1; x++)
            {
                var gx =
                    -image[x - 1, y - 1] + image[x + 1, y - 1] +
                    (-2 * image[x - 1, y]) + (2 * image[x + 1, y]) +
                    -image[x - 1, y + 1] + image[x + 1, y + 1];

                var gy =
                    image[x - 1, y - 1] + (2 * image[x, y - 1]) + image[x + 1, y - 1] -
                    image[x - 1, y + 1] - (2 * image[x, y + 1]) - image[x + 1, y + 1];

                var magnitude = Math.Sqrt((gx * gx) + (gy * gy));
                result[x, y] = (ushort)Math.Min(ushort.MaxValue, Math.Round(magnitude));
            }
        }

        return result;
    }

    public static GrayImage GaussianBlur(GrayImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        int[,] kernel =
        {
            { 1, 2, 1 },
            { 2, 4, 2 },
            { 1, 2, 1 }
        };

        var result = new GrayImage(image.Width, image.Height);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var sum = 0;
                for (var ky = -1; ky <= 1; ky++)
                {
                    for (var kx = -1; kx <= 1; kx++)
                    {
                        sum += SampleClamped(image, x + kx, y + ky) * kernel[ky + 1, kx + 1];
                    }
                }

                result[x, y] = (ushort)Math.Round(sum / 16d);
            }
        }

        return result;
    }

    public static GrayImage MedianFilter(GrayImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var result = new GrayImage(image.Width, image.Height);
        var values = new ushort[9];
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var index = 0;
                for (var ky = -1; ky <= 1; ky++)
                {
                    for (var kx = -1; kx <= 1; kx++)
                    {
                        values[index++] = SampleClamped(image, x + kx, y + ky);
                    }
                }

                Array.Sort(values);
                result[x, y] = values[4];
            }
        }

        return result;
    }

    public static GrayImage Sharpen(GrayImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var result = new GrayImage(image.Width, image.Height);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var value =
                    (5 * image[x, y]) -
                    SampleClamped(image, x - 1, y) -
                    SampleClamped(image, x + 1, y) -
                    SampleClamped(image, x, y - 1) -
                    SampleClamped(image, x, y + 1);

                result[x, y] = ClampToUShort(value);
            }
        }

        return result;
    }

    private static ushort SampleClamped(GrayImage image, int x, int y)
    {
        return image[Clamp(x, 0, image.Width - 1), Clamp(y, 0, image.Height - 1)];
    }

    private static bool SampleBinary(BinaryImage image, int x, int y)
    {
        if (x < 0 || x >= image.Width || y < 0 || y >= image.Height)
        {
            return false;
        }

        return image[x, y];
    }

    private static bool AllNeighborsSet(BinaryImage image, int x, int y)
    {
        for (var ky = -1; ky <= 1; ky++)
        {
            for (var kx = -1; kx <= 1; kx++)
            {
                if (!SampleBinary(image, x + kx, y + ky))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool AnyNeighborSet(BinaryImage image, int x, int y)
    {
        for (var ky = -1; ky <= 1; ky++)
        {
            for (var kx = -1; kx <= 1; kx++)
            {
                if (SampleBinary(image, x + kx, y + ky))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int Clamp(int value, int minimum, int maximum)
    {
        return Math.Min(Math.Max(value, minimum), maximum);
    }

    private static ushort ClampToUShort(int value)
    {
        if (value < 0)
        {
            return 0;
        }

        if (value > ushort.MaxValue)
        {
            return ushort.MaxValue;
        }

        return (ushort)value;
    }
}
