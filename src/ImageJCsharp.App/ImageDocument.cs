using System;
using System.IO;
using ImageJCsharp.Core;

namespace ImageJCsharp.App;

public sealed class ImageDocument
{
    public ImageDocument(string filePath, GrayImage image)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        Image = image ?? throw new ArgumentNullException(nameof(image));
        DisplayName = Path.GetFileName(filePath);
    }

    public string FilePath { get; }

    public string DisplayName { get; }

    public GrayImage Image { get; set; }
}
