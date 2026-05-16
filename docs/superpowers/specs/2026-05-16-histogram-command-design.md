# Histogram Command Design

## Issue

GitHub Issue: #3 Add histogram command

## Goal

Add an ImageJ-style histogram command for grayscale images. The first version should be small, testable, and useful for the MVP without implementing every ImageJ histogram window command.

## Scope

This version includes:

- A core histogram calculation API for `GrayImage`.
- Automated tests for known 8-bit grayscale data.
- `Analyze > Histogram` in the WinForms app.
- A simple ImageJ-style histogram window.
- Histogram calculation for the current rectangle ROI, or the full image when there is no ROI.

This version defers:

- `List`, `Copy`, `Log`, and `Live` button behavior.
- 16-bit optimized display and binning.
- RGB or per-channel histograms.
- Exporting histogram data.
- Histogram options dialogs.

## Core Design

Add a core type named `HistogramResult`.

The result should expose:

- `Bins`: 256 counts for 8-bit gray values.
- `Count`: total pixels included.
- `Mean`: mean gray value.
- `StandardDeviation`: population standard deviation.
- `Min`: minimum gray value.
- `Max`: maximum gray value.
- `Mode`: gray value with the largest count.

Add a calculation API named `Histogram`:

```csharp
Histogram.Calculate(GrayImage image)
Histogram.Calculate(GrayImage image, RectRoi roi)
```

The no-ROI overload calculates the full image. The ROI overload calculates only pixels inside the rectangle ROI. The API should validate inputs clearly and fail fast.

Because the first feature is scoped to 8-bit grayscale data, any pixel value greater than 255 should throw an understandable exception instead of being clipped or silently mapped into a bin.

## UI Design

Add `Analyze > Histogram` with the `H` shortcut.

When the command runs:

- If no image is open, return without opening a window.
- If a rectangle ROI exists, calculate the histogram for that ROI.
- If no ROI exists, calculate the histogram for the full image.
- Open a separate WinForms dialog/window titled `Histogram of <image name>`.

The histogram window should follow ImageJ's basic visual structure:

- White plotting area.
- Black vertical bars for bin counts.
- Gray-scale LUT bar beneath the plot.
- Text showing `Count`, `Mean`, `StdDev`, `Min`, `Max`, and `Mode`.
- Mouse hover over the plot updates a `Value: <bin>, Count: <count>` readout.

The window may display disabled `List`, `Copy`, `Log`, and `Live` buttons only if that helps preserve ImageJ familiarity, but their behavior is not part of this issue. It is also acceptable to omit them in this first version to avoid implying unsupported behavior.

## Error Handling

The core API should throw:

- `ArgumentNullException` for a null image.
- `ArgumentException` or `ArgumentOutOfRangeException` for invalid ROI input.
- `InvalidOperationException` or `NotSupportedException` for pixel values outside the supported 8-bit range.

The UI command should stay quiet when there is no active image, matching existing command behavior.

## Testing

Core tests should be written before production code.

Required tests:

- A small known image produces expected bin counts.
- Statistics are correct for a known image.
- ROI histogram uses only pixels inside the ROI.
- Pixel values above 255 fail clearly.

Verification commands:

```powershell
dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj
dotnet build ImageJCsharp.sln
```

The app UI should be build-verified. Manual smoke testing should confirm that `Analyze > Histogram` opens a readable histogram window for a sample image.

## Acceptance Criteria

- Histogram calculation works for 8-bit grayscale data.
- Tests cover a small known image with known bin counts.
- User can run `Analyze > Histogram`.
- Histogram UI clearly shows the intensity distribution.
- ROI behavior matches the current selection when present.
- Unsupported values fail clearly rather than being silently corrected.
