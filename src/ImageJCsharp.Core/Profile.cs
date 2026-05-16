using System;
using System.Linq;

namespace ImageJCsharp.Core;

public sealed class ProfileResult
{
    public ProfileResult(ushort[] values)
    {
        Values = values?.ToArray() ?? throw new ArgumentNullException(nameof(values));
    }

    public ushort[] Values { get; }
}

public static class Profile
{
    public static ProfileResult HorizontalCenterLine(GrayImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        return HorizontalCenterLine(image, new RectRoi(0, 0, image.Width, image.Height));
    }

    public static ProfileResult HorizontalCenterLine(GrayImage image, RectRoi roi)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var startX = Math.Max(0, roi.X);
        var endX = Math.Min(image.Width, roi.Right);
        var startY = Math.Max(0, roi.Y);
        var endY = Math.Min(image.Height, roi.Bottom);
        if (startX >= endX || startY >= endY)
        {
            throw new ArgumentException("ROI does not intersect the image.", nameof(roi));
        }

        var y = startY + ((endY - startY) / 2);
        var values = new ushort[endX - startX];
        for (var x = startX; x < endX; x++)
        {
            values[x - startX] = image[x, y];
        }

        return new ProfileResult(values);
    }

    public static ProfileResult Line(GrayImage image, LineRoi roi)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var values = new System.Collections.Generic.List<ushort>();
        var x = roi.X1;
        var y = roi.Y1;
        var dx = Math.Abs(roi.X2 - roi.X1);
        var sx = roi.X1 < roi.X2 ? 1 : -1;
        var dy = -Math.Abs(roi.Y2 - roi.Y1);
        var sy = roi.Y1 < roi.Y2 ? 1 : -1;
        var error = dx + dy;

        while (true)
        {
            if ((uint)x >= (uint)image.Width || (uint)y >= (uint)image.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(roi), "Line ROI must be inside the image bounds.");
            }

            values.Add(image[x, y]);
            if (x == roi.X2 && y == roi.Y2)
            {
                break;
            }

            var doubledError = 2 * error;
            if (doubledError >= dy)
            {
                error += dy;
                x += sx;
            }

            if (doubledError <= dx)
            {
                error += dx;
                y += sy;
            }
        }

        return new ProfileResult(values.ToArray());
    }
}
