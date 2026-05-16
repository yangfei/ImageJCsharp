using ImageJCsharp.Core;

namespace ImageJCsharp.Core.Tests;

public sealed class CoreImageTests
{
    [Fact]
    public void GrayImageStoresPixelsInRowMajorOrder()
    {
        var image = new GrayImage(3, 2);

        image[2, 1] = 42;

        Assert.Equal(3, image.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(42, image[2, 1]);
        Assert.Equal(0, image[0, 0]);
    }

    [Fact]
    public void MeasureComputesStatisticsInsideRectRoi()
    {
        var image = GrayImage.FromPixels(3, 2, new ushort[]
        {
            10, 20, 30,
            40, 50, 60
        });

        var result = Measurements.Measure(image, new RectRoi(1, 0, 2, 2), PixelCalibration.Identity);

        Assert.Equal(4, result.PixelCount);
        Assert.Equal(4, result.Area);
        Assert.Equal(40, result.Mean);
        Assert.Equal(20, result.Min);
        Assert.Equal(60, result.Max);
    }

    [Fact]
    public void ThresholdCreatesBinaryMask()
    {
        var image = GrayImage.FromPixels(3, 1, new ushort[] { 4, 5, 6 });

        var binary = ImageProcessor.Threshold(image, 5, 6);

        Assert.False(binary[0, 0]);
        Assert.True(binary[1, 0]);
        Assert.True(binary[2, 0]);
    }

    [Fact]
    public void InvertFlipsAgainstImageMaximum()
    {
        var image = GrayImage.FromPixels(3, 1, new ushort[] { 0, 100, 255 });

        var inverted = ImageProcessor.Invert(image, 255);

        Assert.Equal(255, inverted[0, 0]);
        Assert.Equal(155, inverted[1, 0]);
        Assert.Equal(0, inverted[2, 0]);
    }

    [Fact]
    public void SobelFindsVerticalEdge()
    {
        var image = GrayImage.FromPixels(5, 3, new ushort[]
        {
            0, 0, 255, 255, 255,
            0, 0, 255, 255, 255,
            0, 0, 255, 255, 255
        });

        var edges = ImageProcessor.SobelEdges(image);

        Assert.True(edges[1, 1] > 0);
        Assert.True(edges[2, 1] > 0);
        Assert.Equal(0, edges[0, 0]);
    }
}
