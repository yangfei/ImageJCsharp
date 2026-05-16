using System;
using ImageJCsharp.Core;

namespace ImageJCsharp.App;

public static class BinaryImageConversion
{
    public static BinaryImage FromGrayImage(GrayImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var binary = new BinaryImage(image.Width, image.Height);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var value = image[x, y];
                if (value == 0)
                {
                    binary[x, y] = false;
                }
                else if (value == 255)
                {
                    binary[x, y] = true;
                }
                else
                {
                    throw new NotSupportedException("Binary morphology requires an image with pixel values 0 or 255.");
                }
            }
        }

        return binary;
    }

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
