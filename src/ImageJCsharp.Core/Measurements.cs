using System;

namespace ImageJCsharp.Core;

public readonly struct PixelCalibration
{
    public PixelCalibration(double pixelWidth, double pixelHeight, string unit)
    {
        if (pixelWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pixelWidth), "Pixel width must be positive.");
        }

        if (pixelHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pixelHeight), "Pixel height must be positive.");
        }

        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        Unit = unit ?? "pixel";
    }

    public static PixelCalibration Identity { get; } = new PixelCalibration(1, 1, "pixel");

    public double PixelWidth { get; }

    public double PixelHeight { get; }

    public string Unit { get; }
}

public readonly struct MeasurementResult
{
    public MeasurementResult(int pixelCount, double area, double mean, double min, double max, double standardDeviation)
    {
        PixelCount = pixelCount;
        Area = area;
        Mean = mean;
        Min = min;
        Max = max;
        StandardDeviation = standardDeviation;
    }

    public int PixelCount { get; }

    public double Area { get; }

    public double Mean { get; }

    public double Min { get; }

    public double Max { get; }

    public double StandardDeviation { get; }
}

public static class Measurements
{
    public static MeasurementResult Measure(GrayImage image, RectRoi roi, PixelCalibration calibration)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var startX = Math.Max(0, roi.X);
        var startY = Math.Max(0, roi.Y);
        var endX = Math.Min(image.Width, roi.Right);
        var endY = Math.Min(image.Height, roi.Bottom);

        if (startX >= endX || startY >= endY)
        {
            throw new ArgumentException("ROI does not intersect the image.", nameof(roi));
        }

        var count = 0;
        var sum = 0d;
        var sumSquares = 0d;
        var min = double.MaxValue;
        var max = double.MinValue;

        for (var y = startY; y < endY; y++)
        {
            for (var x = startX; x < endX; x++)
            {
                var value = image[x, y];
                count++;
                sum += value;
                sumSquares += value * value;
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
        }

        var mean = sum / count;
        var variance = Math.Max(0, (sumSquares / count) - (mean * mean));
        var area = count * calibration.PixelWidth * calibration.PixelHeight;

        return new MeasurementResult(count, area, mean, min, max, Math.Sqrt(variance));
    }
}
