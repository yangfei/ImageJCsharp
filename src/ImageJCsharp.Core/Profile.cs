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
}
