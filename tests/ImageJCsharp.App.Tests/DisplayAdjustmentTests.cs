using ImageJCsharp.Core;
using System.Drawing;

namespace ImageJCsharp.App.Tests;

public sealed class DisplayAdjustmentTests
{
    [Fact]
    public void ToBitmapMapsDisplayMinimumAndMaximumToBlackAndWhite()
    {
        var image = GrayImage.FromPixels(3, 1, new ushort[] { 10, 15, 20 });
        var adjustment = new DisplayAdjustment(10, 20);

        using var bitmap = BitmapConversion.ToBitmap(image, adjustment);

        Assert.Equal(Color.FromArgb(0, 0, 0).ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
        Assert.Equal(Color.FromArgb(128, 128, 128).ToArgb(), bitmap.GetPixel(1, 0).ToArgb());
        Assert.Equal(Color.FromArgb(255, 255, 255).ToArgb(), bitmap.GetPixel(2, 0).ToArgb());
        Assert.Equal(10, image[0, 0]);
        Assert.Equal(20, image[2, 0]);
    }

    [Fact]
    public void DisplayAdjustmentRejectsInvalidRange()
    {
        Assert.Throws<ArgumentException>(() => new DisplayAdjustment(20, 10));
    }
}
