using System;

namespace ImageJCsharp.Core;

public sealed class HistogramResult
{
    public HistogramResult(int[] bins, int count, double mean, double standardDeviation, ushort min, ushort max, ushort mode)
    {
        if (bins is null)
        {
            throw new ArgumentNullException(nameof(bins));
        }

        if (bins.Length != 256)
        {
            throw new ArgumentException("Histogram bins must contain 256 entries.", nameof(bins));
        }

        Bins = new int[bins.Length];
        Array.Copy(bins, Bins, bins.Length);
        Count = count;
        Mean = mean;
        StandardDeviation = standardDeviation;
        Min = min;
        Max = max;
        Mode = mode;
    }

    public int[] Bins { get; }

    public int Count { get; }

    public double Mean { get; }

    public double StandardDeviation { get; }

    public ushort Min { get; }

    public ushort Max { get; }

    public ushort Mode { get; }
}
