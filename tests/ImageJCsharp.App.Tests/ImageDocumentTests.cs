using ImageJCsharp.Core;

namespace ImageJCsharp.App.Tests;

public sealed class ImageDocumentTests
{
    [Fact]
    public void ImageDocumentUsesPixelCalibrationByDefault()
    {
        var document = new ImageDocument("cell.png", new GrayImage(1, 1));

        Assert.Equal(1, document.Calibration.PixelWidth);
        Assert.Equal(1, document.Calibration.PixelHeight);
        Assert.Equal("pixel", document.Calibration.Unit);
    }

    [Fact]
    public void ImageDocumentStoresPhysicalCalibration()
    {
        var document = new ImageDocument("cell.png", new GrayImage(1, 1));

        document.Calibration = new PixelCalibration(0.25, 0.5, "um");

        Assert.Equal(0.25, document.Calibration.PixelWidth);
        Assert.Equal(0.5, document.Calibration.PixelHeight);
        Assert.Equal("um", document.Calibration.Unit);
    }
}
