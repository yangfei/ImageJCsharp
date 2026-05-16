namespace ImageJCsharp.App.Tests;

public sealed class MeasurementOptionsTests
{
    [Fact]
    public void MeasurementOptionsDefaultPreservesCurrentResultColumns()
    {
        var options = MeasurementOptions.Default;

        Assert.Equal(
            new[] { "Name", "Area", "Unit", "Mean", "Min", "Max", "StdDev" },
            options.GetColumnNames());
    }

    [Fact]
    public void MeasurementOptionsCanLimitResultColumns()
    {
        var options = new MeasurementOptions(
            showArea: true,
            showMean: false,
            showMin: false,
            showMax: true,
            showStandardDeviation: false);

        Assert.Equal(
            new[] { "Name", "Area", "Unit", "Max" },
            options.GetColumnNames());
    }
}
