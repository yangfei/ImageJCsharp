using System;
using System.Drawing;
using System.Drawing.Imaging;
using ImageJCsharp.Core;

namespace ImageJCsharp.App;

public static class BitmapConversion
{
    public static GrayImage ToGrayImage(Bitmap bitmap)
    {
        if (bitmap is null)
        {
            throw new ArgumentNullException(nameof(bitmap));
        }

        var image = new GrayImage(bitmap.Width, bitmap.Height);
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                image[x, y] = (ushort)Math.Round((color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114));
            }
        }

        return image;
    }

    public static Bitmap ToBitmap(GrayImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
        var max = FindMaximum(image);
        var scale = max <= 255 ? 1d : 255d / max;

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var value = (int)Math.Max(0, Math.Min(255, Math.Round(image[x, y] * scale)));
                bitmap.SetPixel(x, y, Color.FromArgb(value, value, value));
            }
        }

        return bitmap;
    }

    private static ushort FindMaximum(GrayImage image)
    {
        ushort max = 0;
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                max = Math.Max(max, image[x, y]);
            }
        }

        return max == 0 ? (ushort)255 : max;
    }
}
