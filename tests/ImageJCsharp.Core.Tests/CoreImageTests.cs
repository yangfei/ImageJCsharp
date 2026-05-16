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
    public void GrayImageCopiesPixelsInRowMajorOrder()
    {
        var image = GrayImage.FromPixels(3, 2, new ushort[]
        {
            1, 2, 3,
            4, 5, 6
        });

        var pixels = image.CopyPixels();

        Assert.Equal(new ushort[]
        {
            1, 2, 3,
            4, 5, 6
        }, pixels);
    }

    [Fact]
    public void GrayImageCopyPixelsDoesNotExposeInternalBuffer()
    {
        var image = GrayImage.FromPixels(2, 1, new ushort[] { 10, 20 });

        var pixels = image.CopyPixels();
        pixels[0] = 99;

        Assert.Equal(10, image[0, 0]);
        Assert.Equal(new ushort[] { 10, 20 }, image.CopyPixels());
    }

    [Fact]
    public void GrayImageAllowsPixelAccessAtImageCorners()
    {
        var image = GrayImage.FromPixels(3, 2, new ushort[]
        {
            1, 2, 3,
            4, 5, 6
        });

        Assert.Equal(1, image[0, 0]);
        Assert.Equal(3, image[2, 0]);
        Assert.Equal(4, image[0, 1]);
        Assert.Equal(6, image[2, 1]);
    }

    [Fact]
    public void GrayImageRejectsNegativePixelCoordinates()
    {
        var image = new GrayImage(3, 2);

        var xException = Assert.Throws<ArgumentOutOfRangeException>(() => image[-1, 0]);
        var yException = Assert.Throws<ArgumentOutOfRangeException>(() => image[0, -1]);

        Assert.Equal("x", xException.ParamName);
        Assert.Equal("y", yException.ParamName);
    }

    [Fact]
    public void GrayImageRejectsPixelCoordinatesAtOrBeyondImageSize()
    {
        var image = new GrayImage(3, 2);

        var xException = Assert.Throws<ArgumentOutOfRangeException>(() => image[3, 0]);
        var yException = Assert.Throws<ArgumentOutOfRangeException>(() => image[0, 2]);

        Assert.Equal("x", xException.ParamName);
        Assert.Equal("y", yException.ParamName);
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
    public void ThresholdIncludesMinimumAndMaximumBoundaryValues()
    {
        var image = GrayImage.FromPixels(5, 1, new ushort[] { 9, 10, 15, 20, 21 });

        var binary = ImageProcessor.Threshold(image, 10, 20);

        Assert.False(binary[0, 0]);
        Assert.True(binary[1, 0]);
        Assert.True(binary[2, 0]);
        Assert.True(binary[3, 0]);
        Assert.False(binary[4, 0]);
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

    [Fact]
    public void HistogramCountsKnownEightBitValues()
    {
        var image = GrayImage.FromPixels(4, 2, new ushort[]
        {
            0, 1, 1, 2,
            2, 2, 255, 255
        });

        var histogram = Histogram.Calculate(image);

        Assert.Equal(8, histogram.Count);
        Assert.Equal(1, histogram.Bins[0]);
        Assert.Equal(2, histogram.Bins[1]);
        Assert.Equal(3, histogram.Bins[2]);
        Assert.Equal(2, histogram.Bins[255]);
        Assert.Equal(2, histogram.Mode);
    }

    [Fact]
    public void HistogramComputesStatisticsForKnownEightBitValues()
    {
        var image = GrayImage.FromPixels(4, 1, new ushort[] { 0, 10, 10, 20 });

        var histogram = Histogram.Calculate(image);

        Assert.Equal(4, histogram.Count);
        Assert.Equal(10, histogram.Mean);
        Assert.Equal(0, histogram.Min);
        Assert.Equal(20, histogram.Max);
        Assert.Equal(10, histogram.Mode);
        Assert.Equal(Math.Sqrt(50), histogram.StandardDeviation, 6);
    }

    [Fact]
    public void HistogramUsesOnlyPixelsInsideRectRoi()
    {
        var image = GrayImage.FromPixels(3, 2, new ushort[]
        {
            1, 2, 3,
            4, 5, 6
        });

        var histogram = Histogram.Calculate(image, new RectRoi(1, 0, 2, 2));

        Assert.Equal(4, histogram.Count);
        Assert.Equal(0, histogram.Bins[1]);
        Assert.Equal(1, histogram.Bins[2]);
        Assert.Equal(1, histogram.Bins[3]);
        Assert.Equal(1, histogram.Bins[5]);
        Assert.Equal(1, histogram.Bins[6]);
        Assert.Equal(4, histogram.Mean);
        Assert.Equal(2, histogram.Min);
        Assert.Equal(6, histogram.Max);
    }

    [Fact]
    public void HistogramRejectsValuesOutsideEightBitRange()
    {
        var image = GrayImage.FromPixels(1, 1, new ushort[] { 256 });

        var exception = Assert.Throws<NotSupportedException>(() => Histogram.Calculate(image));

        Assert.Contains("8-bit", exception.Message);
    }
}
