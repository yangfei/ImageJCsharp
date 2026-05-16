using System.Collections.Generic;

namespace ImageJCsharp.App;

public sealed class MeasurementOptions
{
    public MeasurementOptions(
        bool showArea,
        bool showMean,
        bool showMin,
        bool showMax,
        bool showStandardDeviation)
    {
        ShowArea = showArea;
        ShowMean = showMean;
        ShowMin = showMin;
        ShowMax = showMax;
        ShowStandardDeviation = showStandardDeviation;
    }

    public static MeasurementOptions Default { get; } = new MeasurementOptions(
        showArea: true,
        showMean: true,
        showMin: true,
        showMax: true,
        showStandardDeviation: true);

    public bool ShowArea { get; }

    public bool ShowMean { get; }

    public bool ShowMin { get; }

    public bool ShowMax { get; }

    public bool ShowStandardDeviation { get; }

    public IReadOnlyList<string> GetColumnNames()
    {
        var columns = new List<string> { "Name" };
        if (ShowArea)
        {
            columns.Add("Area");
            columns.Add("Unit");
        }

        if (ShowMean)
        {
            columns.Add("Mean");
        }

        if (ShowMin)
        {
            columns.Add("Min");
        }

        if (ShowMax)
        {
            columns.Add("Max");
        }

        if (ShowStandardDeviation)
        {
            columns.Add("StdDev");
        }

        return columns;
    }
}
