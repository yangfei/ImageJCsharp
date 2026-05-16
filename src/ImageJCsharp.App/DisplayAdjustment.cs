using System;

namespace ImageJCsharp.App;

public readonly struct DisplayAdjustment
{
    public DisplayAdjustment(ushort minimum, ushort maximum)
    {
        if (minimum >= maximum)
        {
            throw new ArgumentException("Display minimum must be less than display maximum.");
        }

        Minimum = minimum;
        Maximum = maximum;
    }

    public ushort Minimum { get; }

    public ushort Maximum { get; }
}
