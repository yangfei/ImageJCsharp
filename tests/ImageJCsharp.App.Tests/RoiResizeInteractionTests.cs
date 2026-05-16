using System.Drawing;
using ImageJCsharp.Core;

namespace ImageJCsharp.App.Tests;

public sealed class RoiResizeInteractionTests
{
    [Fact]
    public void HitTestFindsSouthEastHandleAtCurrentZoom()
    {
        var roi = new RectRoi(10, 20, 30, 40);

        var handle = RoiResizeInteraction.HitTestHandle(roi, new Point(80, 120), 2d);

        Assert.Equal(RoiResizeHandle.SouthEast, handle);
    }

    [Fact]
    public void ResizeFromSouthEastHandleClampsToImageBounds()
    {
        var roi = new RectRoi(10, 10, 20, 20);

        var resized = RoiResizeInteraction.Resize(roi, RoiResizeHandle.SouthEast, new Point(150, 100), 100, 90);

        AssertRoi(10, 10, 90, 80, resized);
    }

    [Fact]
    public void ResizeFromNorthWestHandleCanCrossOverAndKeepsRoiValid()
    {
        var roi = new RectRoi(10, 10, 20, 20);

        var resized = RoiResizeInteraction.Resize(roi, RoiResizeHandle.NorthWest, new Point(40, 35), 100, 90);

        AssertRoi(29, 29, 12, 7, resized);
    }

    private static void AssertRoi(int x, int y, int width, int height, RectRoi roi)
    {
        Assert.Equal(x, roi.X);
        Assert.Equal(y, roi.Y);
        Assert.Equal(width, roi.Width);
        Assert.Equal(height, roi.Height);
    }
}
