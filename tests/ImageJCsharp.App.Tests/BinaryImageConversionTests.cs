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
}
