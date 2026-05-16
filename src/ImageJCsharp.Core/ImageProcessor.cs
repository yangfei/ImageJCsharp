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
}
