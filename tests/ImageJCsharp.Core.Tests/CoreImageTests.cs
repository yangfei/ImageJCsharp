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
    public void MeasureUsesImageJCompatibleSampleStandardDeviation()
    {
        var image = GrayImage.FromPixels(4, 3, new ushort[]
        {
            0, 10, 20, 30,
            40, 50, 60, 70,
            80, 90, 100, 110
        });

        var result = Measurements.Measure(image, new RectRoi(0, 0, 4, 3), PixelCalibration.Identity);

        Assert.Equal(36.05551275463989, result.StandardDeviation, 12);
    }

    [Fact]
    public void MeasureRoiUsesImageJCompatibleSampleStandardDeviation()
    {
        var image = GrayImage.FromPixels(4, 3, new ushort[]
        {
            0, 10, 20, 30,
            40, 50, 60, 70,
            80, 90, 100, 110
        });

        var result = Measurements.Measure(image, new RectRoi(1, 1, 2, 2), PixelCalibration.Identity);

        Assert.Equal(23.804761428476166, result.StandardDeviation, 12);
    }

    [Fact]
    public void MeasureUsesPixelCalibrationForArea()
    {
        var image = GrayImage.FromPixels(2, 2, new ushort[]
        {
            1, 2,
            3, 4
        });
        var calibration = new PixelCalibration(0.5, 2, "um");

        var result = Measurements.Measure(image, new RectRoi(0, 0, 2, 2), calibration);

        Assert.Equal(4, result.PixelCount);
        Assert.Equal(4, result.Area);
    }

    [Fact]
    public void MeasureUsesOnlyPixelsInsideOvalRoi()
    {
        var image = GrayImage.FromPixels(3, 3, new ushort[]
        {
            1, 2, 3,
            4, 5, 6,
            7, 8, 9
        });

        var result = Measurements.Measure(image, new OvalRoi(0, 0, 3, 3), PixelCalibration.Identity);

        Assert.Equal(5, result.PixelCount);
        Assert.Equal(5, result.Area);
        Assert.Equal(5, result.Mean);
        Assert.Equal(2, result.Min);
        Assert.Equal(8, result.Max);
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
    public void BinaryDilateExpandsSinglePixelWithThreeByThreeNeighborhood()
    {
        var image = CreateBinaryImage(3, 3, new[]
        {
            false, false, false,
            false, true, false,
            false, false, false
        });

        var dilated = ImageProcessor.Dilate(image);

        AssertBinaryEquals(dilated, new[]
        {
            true, true, true,
            true, true, true,
            true, true, true
        });
    }

    [Fact]
    public void BinaryErodeRequiresFullThreeByThreeNeighborhood()
    {
        var image = CreateBinaryImage(5, 5, new[]
        {
            false, false, false, false, false,
            false, true, true, true, false,
            false, true, true, true, false,
            false, true, true, true, false,
            false, false, false, false, false
        });

        var eroded = ImageProcessor.Erode(image);

        AssertBinaryEquals(eroded, new[]
        {
            false, false, false, false, false,
            false, false, false, false, false,
            false, false, true, false, false,
            false, false, false, false, false,
            false, false, false, false, false
        });
    }

    [Fact]
    public void BinaryOpenRemovesIsolatedPixel()
    {
        var image = CreateBinaryImage(3, 3, new[]
        {
            false, false, false,
            false, true, false,
            false, false, false
        });

        var opened = ImageProcessor.Open(image);

        AssertBinaryEquals(opened, new[]
        {
            false, false, false,
            false, false, false,
            false, false, false
        });
    }

    [Fact]
    public void BinaryCloseFillsSmallHole()
    {
        var image = CreateBinaryImage(5, 5, new[]
        {
            false, false, false, false, false,
            false, true, true, true, false,
            false, true, false, true, false,
            false, true, true, true, false,
            false, false, false, false, false
        });

        var closed = ImageProcessor.Close(image);

        Assert.True(closed[2, 2]);
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
    public void GaussianBlurUsesThreeByThreeWeightedKernel()
    {
        var image = GrayImage.FromPixels(3, 3, new ushort[]
        {
            0, 0, 0,
            0, 160, 0,
            0, 0, 0
        });

        var blurred = ImageProcessor.GaussianBlur(image);

        Assert.Equal(40, blurred[1, 1]);
        Assert.Equal(20, blurred[0, 1]);
        Assert.Equal(10, blurred[0, 0]);
    }

    [Fact]
    public void MedianFilterReplacesCenterWithNeighborhoodMedian()
    {
        var image = GrayImage.FromPixels(3, 3, new ushort[]
        {
            10, 10, 10,
            10, 250, 10,
            10, 10, 10
        });

        var filtered = ImageProcessor.MedianFilter(image);

        Assert.Equal(10, filtered[1, 1]);
    }

    [Fact]
    public void SharpenEnhancesCenterPixelWithFourNeighborKernel()
    {
        var image = GrayImage.FromPixels(3, 3, new ushort[]
        {
            10, 10, 10,
            10, 20, 10,
            10, 10, 10
        });

        var sharpened = ImageProcessor.Sharpen(image);

        Assert.Equal(60, sharpened[1, 1]);
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

    [Fact]
    public void ProfileUsesHorizontalCenterLineOfImage()
    {
        var image = GrayImage.FromPixels(3, 3, new ushort[]
        {
            1, 2, 3,
            4, 5, 6,
            7, 8, 9
        });

        var profile = Profile.HorizontalCenterLine(image);

        Assert.Equal(new ushort[] { 4, 5, 6 }, profile.Values);
    }

    [Fact]
    public void ProfileUsesHorizontalCenterLineOfRectRoi()
    {
        var image = GrayImage.FromPixels(4, 4, new ushort[]
        {
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16
        });

        var profile = Profile.HorizontalCenterLine(image, new RectRoi(1, 1, 2, 2));

        Assert.Equal(new ushort[] { 10, 11 }, profile.Values);
    }

    [Fact]
    public void ProfileSamplesPixelsAlongLineRoi()
    {
        var image = GrayImage.FromPixels(3, 3, new ushort[]
        {
            1, 2, 3,
            4, 5, 6,
            7, 8, 9
        });

        var profile = Profile.Line(image, new LineRoi(0, 0, 2, 2));

        Assert.Equal(new ushort[] { 1, 5, 9 }, profile.Values);
    }

    private static BinaryImage CreateBinaryImage(int width, int height, bool[] pixels)
    {
        var image = new BinaryImage(width, height);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                image[x, y] = pixels[(y * width) + x];
            }
        }

        return image;
    }

    private static void AssertBinaryEquals(BinaryImage image, bool[] expected)
    {
        Assert.Equal(image.Width * image.Height, expected.Length);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                Assert.Equal(expected[(y * image.Width) + x], image[x, y]);
            }
        }
    }
}
