using ImageJCsharp.Core;

namespace ImageJCsharp.App.Tests;

public sealed class ManagedRoiTests
{
    [Fact]
    public void ManagedRoiFormatsRectangleDisplayName()
    {
        var roi = ManagedRoi.Rectangle(new RectRoi(1, 2, 3, 4));

        Assert.Equal("Rectangle 1,2 3x4", roi.ToString());
    }

    [Fact]
    public void ManagedRoiFormatsLineDisplayName()
    {
        var roi = ManagedRoi.FromLine(new LineRoi(1, 2, 3, 4));

        Assert.Equal("Line 1,2 to 3,4", roi.ToString());
    }
}
