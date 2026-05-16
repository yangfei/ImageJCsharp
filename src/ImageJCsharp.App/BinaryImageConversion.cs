using System;
using ImageJCsharp.Core;

namespace ImageJCsharp.App;

public static class BinaryImageConversion
{
    public static GrayImage ToGrayImage(BinaryImage binary)
    {
        if (binary is null)
        {
            throw new ArgumentNullException(nameof(binary));
        }

        var image = new GrayImage(binary.Width, binary.Height);
        for (var y = 0; y < binary.Height; y++)
        {
            for (var x = 0; x < binary.Width; x++)
            {
                image[x, y] = binary[x, y] ? (ushort)255 : (ushort)0;
            }
        }

        return image;
    }
}
