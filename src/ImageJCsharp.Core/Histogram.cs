using System;

namespace ImageJCsharp.Core;

public static class Histogram
{
    public static HistogramResult Calculate(GrayImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        return Calculate(image, new RectRoi(0, 0, image.Width, image.Height));
    }

    public static HistogramResult Calculate(GrayImage image, RectRoi roi)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (roi.Width <= 0 || roi.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(roi), "ROI dimensions must be positive.");
        }

        if (roi.X < 0 || roi.Y < 0 || roi.Right > image.Width || roi.Bottom > image.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(roi), "ROI must be inside the image bounds.");
        }

        var bins = new int[256];
        var count = 0;
        var sum = 0d;
        var sumSquares = 0d;
        ushort min = 255;
        ushort max = 0;

        for (var y = roi.Y; y < roi.Bottom; y++)
        {
            for (var x = roi.X; x < roi.Right; x++)
            {
                var value = image[x, y];
                if (value > 255)
                {
                    throw new NotSupportedException("Histogram calculation currently supports only 8-bit grayscale values.");
                }

                bins[value]++;
                count++;
                sum += value;
                sumSquares += value * value;
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
        }

        var mean = sum / count;
        var variance = Math.Max(0d, (sumSquares / count) - (mean * mean));

        return new HistogramResult(bins, count, mean, Math.Sqrt(variance), min, max, FindMode(bins));
    }

    private static ushort FindMode(int[] bins)
    {
        var mode = 0;
        var largestCount = bins[0];
        for (var i = 1; i < bins.Length; i++)
        {
            if (bins[i] > largestCount)
            {
                mode = i;
                largestCount = bins[i];
            }
        }

        return (ushort)mode;
    }
}
