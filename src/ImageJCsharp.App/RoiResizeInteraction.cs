using System;
using System.Drawing;
using ImageJCsharp.Core;

namespace ImageJCsharp.App;

public enum RoiResizeHandle
{
    None,
    NorthWest,
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West
}

public static class RoiResizeInteraction
{
    public const int HandleSize = 7;

    public static RoiResizeHandle HitTestHandle(RectRoi roi, Point controlPoint, double zoom)
    {
        foreach (var handle in new[]
        {
            RoiResizeHandle.NorthWest,
            RoiResizeHandle.North,
            RoiResizeHandle.NorthEast,
            RoiResizeHandle.East,
            RoiResizeHandle.SouthEast,
            RoiResizeHandle.South,
            RoiResizeHandle.SouthWest,
            RoiResizeHandle.West
        })
        {
            if (GetHandleBounds(roi, handle, zoom).Contains(controlPoint))
            {
                return handle;
            }
        }

        return RoiResizeHandle.None;
    }

    public static RectRoi Resize(RectRoi roi, RoiResizeHandle handle, Point imagePoint, int imageWidth, int imageHeight)
    {
        if (handle == RoiResizeHandle.None)
        {
            return roi;
        }

        var left = roi.X;
        var top = roi.Y;
        var right = roi.Right - 1;
        var bottom = roi.Bottom - 1;
        var x = Clamp(imagePoint.X, 0, imageWidth - 1);
        var y = Clamp(imagePoint.Y, 0, imageHeight - 1);

        switch (handle)
        {
            case RoiResizeHandle.NorthWest:
                left = x;
                top = y;
                break;
            case RoiResizeHandle.North:
                top = y;
                break;
            case RoiResizeHandle.NorthEast:
                right = x;
                top = y;
                break;
            case RoiResizeHandle.East:
                right = x;
                break;
            case RoiResizeHandle.SouthEast:
                right = x;
                bottom = y;
                break;
            case RoiResizeHandle.South:
                bottom = y;
                break;
            case RoiResizeHandle.SouthWest:
                left = x;
                bottom = y;
                break;
            case RoiResizeHandle.West:
                left = x;
                break;
        }

        var roiLeft = Math.Min(left, right);
        var roiTop = Math.Min(top, bottom);
        var roiRight = Math.Max(left, right);
        var roiBottom = Math.Max(top, bottom);
        return new RectRoi(roiLeft, roiTop, roiRight - roiLeft + 1, roiBottom - roiTop + 1);
    }

    public static Rectangle GetHandleBounds(RectRoi roi, RoiResizeHandle handle, double zoom)
    {
        var center = GetHandleCenter(roi, handle, zoom);
        var offset = HandleSize / 2;
        return new Rectangle(center.X - offset, center.Y - offset, HandleSize, HandleSize);
    }

    private static Point GetHandleCenter(RectRoi roi, RoiResizeHandle handle, double zoom)
    {
        var left = roi.X * zoom;
        var top = roi.Y * zoom;
        var right = roi.Right * zoom;
        var bottom = roi.Bottom * zoom;
        var middleX = (left + right) / 2d;
        var middleY = (top + bottom) / 2d;

        return handle switch
        {
            RoiResizeHandle.NorthWest => RoundPoint(left, top),
            RoiResizeHandle.North => RoundPoint(middleX, top),
            RoiResizeHandle.NorthEast => RoundPoint(right, top),
            RoiResizeHandle.East => RoundPoint(right, middleY),
            RoiResizeHandle.SouthEast => RoundPoint(right, bottom),
            RoiResizeHandle.South => RoundPoint(middleX, bottom),
            RoiResizeHandle.SouthWest => RoundPoint(left, bottom),
            RoiResizeHandle.West => RoundPoint(left, middleY),
            _ => Point.Empty
        };
    }

    private static Point RoundPoint(double x, double y)
    {
        return new Point((int)Math.Round(x), (int)Math.Round(y));
    }

    private static int Clamp(int value, int minimum, int maximum)
    {
        return Math.Max(minimum, Math.Min(maximum, value));
    }
}
