# Histogram Command Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an ImageJ-style `Analyze > Histogram` command for 8-bit grayscale images.

**Architecture:** Core histogram calculation lives in `ImageJCsharp.Core` and is covered by tests. The WinForms app calls the core API and displays a focused histogram window that draws the plot, gray LUT bar, statistics, and hover readout.

**Tech Stack:** C# on .NET Framework 4.8, WinForms, xUnit, `System.Drawing`.

---

## File Structure

- Create `src/ImageJCsharp.Core/HistogramResult.cs`: immutable histogram result object with bins and summary statistics.
- Create `src/ImageJCsharp.Core/Histogram.cs`: histogram calculation API for full image and rectangle ROI.
- Modify `tests/ImageJCsharp.Core.Tests/CoreImageTests.cs`: add failing tests before production code.
- Create `src/ImageJCsharp.App/HistogramForm.cs`: ImageJ-style histogram display window.
- Modify `src/ImageJCsharp.App/Form1.cs`: add `Analyze > Histogram` menu item and command handler.
- Modify `.codex/project-memory.md`: record completed issue after merge/sync only.

---

### Task 1: Core Histogram Tests

**Files:**
- Modify: `tests/ImageJCsharp.Core.Tests/CoreImageTests.cs`

- [ ] **Step 1: Write failing tests**

Append these tests inside `CoreImageTests`:

```csharp
[Fact]
public void HistogramCountsKnownEightBitValues()
{
    var image = GrayImage.FromPixels(4, 2, new ushort[]
    {
        0, 1, 1, 2,
        2, 2, 255, 255
    });

    var histogram = Histogram.Calculate(image);

    Assert.Equal(8, histogram.Count);
    Assert.Equal(1, histogram.Bins[0]);
    Assert.Equal(2, histogram.Bins[1]);
    Assert.Equal(3, histogram.Bins[2]);
    Assert.Equal(2, histogram.Bins[255]);
    Assert.Equal(2, histogram.Mode);
}

[Fact]
public void HistogramComputesStatisticsForKnownEightBitValues()
{
    var image = GrayImage.FromPixels(4, 1, new ushort[] { 0, 10, 10, 20 });

    var histogram = Histogram.Calculate(image);

    Assert.Equal(4, histogram.Count);
    Assert.Equal(10, histogram.Mean);
    Assert.Equal(0, histogram.Min);
    Assert.Equal(20, histogram.Max);
    Assert.Equal(10, histogram.Mode);
    Assert.Equal(Math.Sqrt(50), histogram.StandardDeviation, 6);
}

[Fact]
public void HistogramUsesOnlyPixelsInsideRectRoi()
{
    var image = GrayImage.FromPixels(3, 2, new ushort[]
    {
        1, 2, 3,
        4, 5, 6
    });

    var histogram = Histogram.Calculate(image, new RectRoi(1, 0, 2, 2));

    Assert.Equal(4, histogram.Count);
    Assert.Equal(0, histogram.Bins[1]);
    Assert.Equal(1, histogram.Bins[2]);
    Assert.Equal(1, histogram.Bins[3]);
    Assert.Equal(1, histogram.Bins[5]);
    Assert.Equal(1, histogram.Bins[6]);
    Assert.Equal(4, histogram.Mean);
    Assert.Equal(2, histogram.Min);
    Assert.Equal(6, histogram.Max);
}

[Fact]
public void HistogramRejectsValuesOutsideEightBitRange()
{
    var image = GrayImage.FromPixels(1, 1, new ushort[] { 256 });

    var exception = Assert.Throws<NotSupportedException>(() => Histogram.Calculate(image));

    Assert.Contains("8-bit", exception.Message);
}
```

- [ ] **Step 2: Run core tests and verify RED**

Run:

```powershell
dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj
```

Expected: compile failure because `Histogram` does not exist.

- [ ] **Step 3: Commit is not allowed yet**

Do not commit failing tests alone unless stopping work. Continue to Task 2.

---

### Task 2: Core Histogram Implementation

**Files:**
- Create: `src/ImageJCsharp.Core/HistogramResult.cs`
- Create: `src/ImageJCsharp.Core/Histogram.cs`

- [ ] **Step 1: Create `HistogramResult`**

Create `src/ImageJCsharp.Core/HistogramResult.cs`:

```csharp
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
```

- [ ] **Step 2: Create `Histogram`**

Create `src/ImageJCsharp.Core/Histogram.cs`:

```csharp
using System;

namespace ImageJCsharp.Core;

public static class Histogram
{
    public static HistogramResult Calculate(GrayImage image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        return Calculate(image, new RectRoi(0, 0, image.Width, image.Height));
    }

    public static HistogramResult Calculate(GrayImage image, RectRoi roi)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (roi.Width <= 0 || roi.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(roi), "ROI dimensions must be positive.");
        }

        if (roi.X < 0 || roi.Y < 0 || roi.Right > image.Width || roi.Bottom > image.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(roi), "ROI must be inside the image bounds.");
        }

        var bins = new int[256];
        var count = 0;
        var sum = 0d;
        var sumSquares = 0d;
        ushort min = 255;
        ushort max = 0;

        for (var y = roi.Y; y < roi.Bottom; y++)
        {
            for (var x = roi.X; x < roi.Right; x++)
            {
                var value = image[x, y];
                if (value > 255)
                {
                    throw new NotSupportedException("Histogram calculation currently supports only 8-bit grayscale values.");
                }

                bins[value]++;
                count++;
                sum += value;
                sumSquares += value * value;
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
        }

        var mean = sum / count;
        var variance = Math.Max(0d, (sumSquares / count) - (mean * mean));
        var mode = FindMode(bins);

        return new HistogramResult(bins, count, mean, Math.Sqrt(variance), min, max, mode);
    }

    private static ushort FindMode(int[] bins)
    {
        var mode = 0;
        var largestCount = bins[0];
        for (var i = 1; i < bins.Length; i++)
        {
            if (bins[i] > largestCount)
            {
                mode = i;
                largestCount = bins[i];
            }
        }

        return (ushort)mode;
    }
}
```

- [ ] **Step 3: Run core tests and verify GREEN**

Run:

```powershell
dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj
```

Expected: all core tests pass.

- [ ] **Step 4: Commit core histogram**

Run:

```powershell
git add src/ImageJCsharp.Core/HistogramResult.cs src/ImageJCsharp.Core/Histogram.cs tests/ImageJCsharp.Core.Tests/CoreImageTests.cs
git commit -m "feat: add core histogram calculation"
```

---

### Task 3: Histogram Window UI

**Files:**
- Create: `src/ImageJCsharp.App/HistogramForm.cs`

- [ ] **Step 1: Create the histogram form**

Create `src/ImageJCsharp.App/HistogramForm.cs`:

```csharp
using System;
using System.Drawing;
using System.Windows.Forms;
using ImageJCsharp.Core;

namespace ImageJCsharp.App;

public sealed class HistogramForm : Form
{
    private readonly HistogramResult _histogram;
    private readonly Panel _plotPanel = new Panel();
    private readonly Label _statsLabel = new Label();
    private readonly Label _hoverLabel = new Label();

    public HistogramForm(string imageName, HistogramResult histogram)
    {
        if (histogram is null)
        {
            throw new ArgumentNullException(nameof(histogram));
        }

        _histogram = histogram;
        Text = "Histogram of " + imageName;
        ClientSize = new Size(420, 320);
        FormBorderStyle = FormBorderStyle.SizableToolWindow;
        StartPosition = FormStartPosition.CenterParent;
        BuildUi();
        UpdateStats();
    }

    private void BuildUi()
    {
        _plotPanel.Dock = DockStyle.Top;
        _plotPanel.Height = 220;
        _plotPanel.BackColor = Color.White;
        _plotPanel.Paint += PlotPanelPaint;
        _plotPanel.MouseMove += PlotPanelMouseMove;
        _plotPanel.MouseLeave += (_, _) => _hoverLabel.Text = "Value: -, Count: -";
        Controls.Add(_plotPanel);

        _statsLabel.Dock = DockStyle.Top;
        _statsLabel.Height = 55;
        _statsLabel.Padding = new Padding(8, 6, 8, 0);
        Controls.Add(_statsLabel);

        _hoverLabel.Dock = DockStyle.Top;
        _hoverLabel.Height = 28;
        _hoverLabel.Padding = new Padding(8, 4, 8, 0);
        _hoverLabel.Text = "Value: -, Count: -";
        Controls.Add(_hoverLabel);
    }

    private void UpdateStats()
    {
        _statsLabel.Text =
            $"Count: {_histogram.Count}    Mean: {_histogram.Mean:0.###}    StdDev: {_histogram.StandardDeviation:0.###}\r\n" +
            $"Min: {_histogram.Min}    Max: {_histogram.Max}    Mode: {_histogram.Mode}";
    }

    private void PlotPanelPaint(object? sender, PaintEventArgs e)
    {
        var bounds = _plotPanel.ClientRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var plotBounds = new Rectangle(36, 12, Math.Max(1, bounds.Width - 52), Math.Max(1, bounds.Height - 44));
        var lutBounds = new Rectangle(plotBounds.Left, plotBounds.Bottom + 8, plotBounds.Width, 12);
        var maxCount = Math.Max(1, GetMaxBinCount());

        e.Graphics.Clear(Color.White);
        using var axisPen = new Pen(Color.Black);
        e.Graphics.DrawRectangle(axisPen, plotBounds);

        using var barBrush = new SolidBrush(Color.Black);
        for (var i = 0; i < _histogram.Bins.Length; i++)
        {
            var x = plotBounds.Left + (int)Math.Floor((double)i * plotBounds.Width / _histogram.Bins.Length);
            var nextX = plotBounds.Left + (int)Math.Floor((double)(i + 1) * plotBounds.Width / _histogram.Bins.Length);
            var barWidth = Math.Max(1, nextX - x);
            var barHeight = (int)Math.Round((double)_histogram.Bins[i] * plotBounds.Height / maxCount);
            e.Graphics.FillRectangle(barBrush, x, plotBounds.Bottom - barHeight, barWidth, barHeight);
        }

        for (var x = 0; x < lutBounds.Width; x++)
        {
            var value = (int)Math.Round((double)x * 255 / Math.Max(1, lutBounds.Width - 1));
            using var pen = new Pen(Color.FromArgb(value, value, value));
            e.Graphics.DrawLine(pen, lutBounds.Left + x, lutBounds.Top, lutBounds.Left + x, lutBounds.Bottom);
        }

        e.Graphics.DrawRectangle(axisPen, lutBounds);
        e.Graphics.DrawString("0", Font, Brushes.Black, plotBounds.Left - 3, lutBounds.Bottom + 2);
        e.Graphics.DrawString("255", Font, Brushes.Black, plotBounds.Right - 22, lutBounds.Bottom + 2);
    }

    private void PlotPanelMouseMove(object? sender, MouseEventArgs e)
    {
        var bounds = _plotPanel.ClientRectangle;
        var plotBounds = new Rectangle(36, 12, Math.Max(1, bounds.Width - 52), Math.Max(1, bounds.Height - 44));
        if (!plotBounds.Contains(e.Location))
        {
            _hoverLabel.Text = "Value: -, Count: -";
            return;
        }

        var value = Math.Max(0, Math.Min(255, (int)Math.Floor((double)(e.X - plotBounds.Left) * 256 / plotBounds.Width)));
        _hoverLabel.Text = $"Value: {value}, Count: {_histogram.Bins[value]}";
    }

    private int GetMaxBinCount()
    {
        var max = 0;
        foreach (var count in _histogram.Bins)
        {
            max = Math.Max(max, count);
        }

        return max;
    }
}
```

- [ ] **Step 2: Build app to catch compile errors**

Run:

```powershell
dotnet build src/ImageJCsharp.App/ImageJCsharp.App.csproj
```

Expected: build succeeds.

---

### Task 4: Menu Command Wiring

**Files:**
- Modify: `src/ImageJCsharp.App/Form1.cs`

- [ ] **Step 1: Add menu item**

In `BuildUi`, update the Analyze menu block to:

```csharp
var analyze = AddMenu(menu, "&Analyze");
AddItem(analyze, "&Measure", MeasureCurrentRoi, Keys.Control | Keys.M);
AddItem(analyze, "&Histogram", ShowHistogram, Keys.H);
AddItem(analyze, "Export &Results...", ExportResults, Keys.Control | Keys.E);
```

- [ ] **Step 2: Add command handler**

Add this method near `MeasureCurrentRoi`:

```csharp
private void ShowHistogram()
{
    if (_document is null)
    {
        return;
    }

    var histogram = _roi is null
        ? Histogram.Calculate(_document.Image)
        : Histogram.Calculate(_document.Image, _roi.Value);

    var form = new HistogramForm(_document.DisplayName, histogram);
    form.Show(this);
}
```

- [ ] **Step 3: Build solution**

Run:

```powershell
dotnet build ImageJCsharp.sln
```

Expected: build succeeds with 0 errors.

- [ ] **Step 4: Commit UI wiring**

Run:

```powershell
git add src/ImageJCsharp.App/HistogramForm.cs src/ImageJCsharp.App/Form1.cs
git commit -m "feat: add histogram window"
```

---

### Task 5: Verification and PR

**Files:**
- Modify: `.codex/project-memory.md` after PR merge only.

- [ ] **Step 1: Run full verification**

Run:

```powershell
dotnet build ImageJCsharp.sln
dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj
dotnet test tests/ImageJCsharp.App.Tests/ImageJCsharp.App.Tests.csproj
```

Expected: all commands pass.

- [ ] **Step 2: Manual smoke test**

Run the app, open `samples/shapes-8bit.png`, and confirm:

- `Analyze > Histogram` opens a window.
- The plot has visible bars.
- The gray LUT bar is visible.
- Statistics are readable.
- Hovering over the plot updates `Value` and `Count`.

- [ ] **Step 3: Push branch**

Run:

```powershell
git push -u origin feature/histogram-command
```

- [ ] **Step 4: Create PR**

Run:

```powershell
$env:Path += ';C:\Program Files\GitHub CLI'
gh pr create --repo yangfei/ImageJCsharp --title "Add histogram command" --body "Closes #3`n`n## Summary`n- Add tested 8-bit grayscale histogram calculation in core`n- Add Analyze > Histogram command`n- Add ImageJ-style histogram window with plot, LUT bar, statistics, and hover readout`n`n## Verification`n- dotnet build ImageJCsharp.sln`n- dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj`n- dotnet test tests/ImageJCsharp.App.Tests/ImageJCsharp.App.Tests.csproj"
```

- [ ] **Step 5: Watch checks and merge if clean**

Run:

```powershell
gh pr checks --repo yangfei/ImageJCsharp --watch --interval 5
gh pr merge --repo yangfei/ImageJCsharp --squash --delete-branch
```

- [ ] **Step 6: Sync main and update memory**

Run:

```powershell
git switch main
git pull --ff-only origin main
```

Then update `.codex/project-memory.md` with the completed Issue #3 details and local verification results.

---

## Self-Review

- Spec coverage: core API, tests, Analyze menu command, ImageJ-style UI, ROI/full-image behavior, and 8-bit fast failure are all covered.
- Placeholder scan: no TBD/TODO placeholders are present.
- Type consistency: `Histogram`, `HistogramResult`, `Bins`, `Count`, `Mean`, `StandardDeviation`, `Min`, `Max`, and `Mode` are used consistently across tasks.
