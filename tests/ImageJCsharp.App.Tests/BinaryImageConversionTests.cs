using ImageJCsharp.Core;

namespace ImageJCsharp.App.Tests;

public sealed class BinaryImageConversionTests
{
    [Fact]
    public void ToGrayImageMapsFalseToZeroAndTrueToMaxValue()
    {
        var binary = new BinaryImage(3, 1);
        binary[0, 0] = false;
        binary[1, 0] = true;
        binary[2, 0] = false;

        var image = BinaryImageConversion.ToGrayImage(binary);

        Assert.Equal(3, image.Width);
        Assert.Equal(1, image.Height);
        Assert.Equal(0, image[0, 0]);
        Assert.Equal(255, image[1, 0]);
        Assert.Equal(0, image[2, 0]);
    }

    [Fact]
    public void FromGrayImageAcceptsOnlyZeroAndMaxValue()
    {
        var image = GrayImage.FromPixels(3, 1, new ushort[] { 0, 255, 0 });

        var binary = BinaryImageConversion.FromGrayImage(image);

        Assert.False(binary[0, 0]);
        Assert.True(binary[1, 0]);
        Assert.False(binary[2, 0]);
    }

    [Fact]
    public void FromGrayImageRejectsNonBinaryPixelValues()
    {
        var image = GrayImage.FromPixels(3, 1, new ushort[] { 0, 128, 255 });

        var exception = Assert.Throws<NotSupportedException>(() => BinaryImageConversion.FromGrayImage(image));

        Assert.Contains("0 or 255", exception.Message);
    }
}
