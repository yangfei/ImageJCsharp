using ImageJCsharp.App;
using ImageJCsharp.Core;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ImageJCsharp.App.Tests;

public sealed class FormStartupTests
{
    [Fact]
    public void Form1ConstructsWithoutMenuShortcutExceptions()
    {
        var capturedException = RunOnStaThread(() =>
        {
            using var form = new Form1();
        });
        Assert.Null(capturedException);
    }

    [Fact]
    public void Form1IncludesMinimumImageJLikeTopLevelMenus()
    {
        string[]? menuTexts = null;
        var capturedException = RunOnStaThread(() =>
        {
            using var form = new Form1();
            menuTexts = form.MainMenuStrip?.Items
                .OfType<ToolStripMenuItem>()
                .Select(item => item.Text.Replace("&", string.Empty))
                .ToArray();
        });

        Assert.Null(capturedException);
        Assert.Equal(
            new[] { "File", "Edit", "Image", "Process", "Analyze", "View", "Window", "Help" },
            menuTexts);
    }

    [Fact]
    public void Form1DisablesImageCommandsWhenNoImageIsActive()
    {
        Dictionary<string, bool>? commandStates = null;
        var capturedException = RunOnStaThread(() =>
        {
            using var form = new Form1();
            commandStates = new Dictionary<string, bool>
            {
                ["File/Open"] = FindMenuItem(form, "File", "Open...").Enabled,
                ["File/Save As"] = FindMenuItem(form, "File", "Save As...").Enabled,
                ["File/Close"] = FindMenuItem(form, "File", "Close").Enabled,
                ["File/Exit"] = FindMenuItem(form, "File", "Exit").Enabled,
                ["Process/Invert"] = FindMenuItem(form, "Process", "Invert").Enabled,
                ["Process/Find Edges"] = FindMenuItem(form, "Process", "Find Edges").Enabled,
                ["Process/Gaussian Blur"] = FindMenuItem(form, "Process", "Gaussian Blur").Enabled,
                ["Process/Median"] = FindMenuItem(form, "Process", "Median").Enabled,
                ["Process/Sharpen"] = FindMenuItem(form, "Process", "Sharpen").Enabled,
                ["Process/Erode"] = FindMenuItem(form, "Process", "Erode").Enabled,
                ["Process/Dilate"] = FindMenuItem(form, "Process", "Dilate").Enabled,
                ["Process/Open"] = FindMenuItem(form, "Process", "Open").Enabled,
                ["Process/Close"] = FindMenuItem(form, "Process", "Close").Enabled,
                ["Process/Threshold"] = FindMenuItem(form, "Process", "Threshold...").Enabled,
                ["Analyze/Measure"] = FindMenuItem(form, "Analyze", "Measure").Enabled,
                ["Analyze/Histogram"] = FindMenuItem(form, "Analyze", "Histogram").Enabled,
                ["Analyze/Export Results"] = FindMenuItem(form, "Analyze", "Export Results...").Enabled,
                ["View/Zoom In"] = FindMenuItem(form, "View", "Zoom In").Enabled,
                ["View/Zoom Out"] = FindMenuItem(form, "View", "Zoom Out").Enabled,
                ["View/Actual Size"] = FindMenuItem(form, "View", "Actual Size").Enabled,
                ["View/Fit to Window"] = FindMenuItem(form, "View", "Fit to Window").Enabled
            };
        });

        Assert.Null(capturedException);
        Assert.NotNull(commandStates);
        Assert.True(commandStates["File/Open"]);
        Assert.True(commandStates["File/Exit"]);

        foreach (var state in commandStates.Where(state => state.Key != "File/Open" && state.Key != "File/Exit"))
        {
            Assert.False(state.Value);
        }
    }

    [Fact]
    public void EditMenuCanSelectOvalRoiTool()
    {
        RoiShape? selectedTool = null;
        var capturedException = RunOnStaThread(() =>
        {
            using var form = new Form1();

            FindMenuItem(form, "Edit", "Oval Selection").PerformClick();

            selectedTool = GetPrivateField<RoiShape>(form, "_selectionTool");
        });

        Assert.Null(capturedException);
        Assert.Equal(RoiShape.Oval, selectedTool);
    }

    [Fact]
    public void EditMenuCanSelectLineRoiTool()
    {
        RoiShape? selectedTool = null;
        var capturedException = RunOnStaThread(() =>
        {
            using var form = new Form1();

            FindMenuItem(form, "Edit", "Line Selection").PerformClick();

            selectedTool = GetPrivateField<RoiShape>(form, "_selectionTool");
        });

        Assert.Null(capturedException);
        Assert.Equal(RoiShape.Line, selectedTool);
    }

    [Fact]
    public void FileCloseClearsActiveImageState()
    {
        string? title = null;
        string? statusText = null;
        bool? imageCleared = null;
        bool? imageBoxSizeCleared = null;
        bool? roiCleared = null;
        bool? closeDisabledAfterClose = null;
        Exception? noImageCommandException = null;

        var capturedException = RunOnStaThread(() =>
        {
            using var form = new Form1();
            LoadTestImage(form);

            FindMenuItem(form, "File", "Close").PerformClick();

            title = form.Text;
            statusText = GetStatusText(form);
            imageCleared = GetImageBox(form).Image is null;
            imageBoxSizeCleared = GetImageBox(form).Size == Size.Empty;
            roiCleared = GetPrivateField<RectRoi?>(form, "_roi") is null;
            closeDisabledAfterClose = !FindMenuItem(form, "File", "Close").Enabled;

            try
            {
                InvokePrivateMethod(form, "CloseImage");
                InvokePrivateMethod(form, "ApplyInvert");
                InvokePrivateMethod(form, "ApplyFindEdges");
                InvokePrivateMethod(form, "ApplyGaussianBlur");
                InvokePrivateMethod(form, "ApplyMedianFilter");
                InvokePrivateMethod(form, "ApplySharpen");
                InvokePrivateMethod(form, "ApplyErode");
                InvokePrivateMethod(form, "ApplyDilate");
                InvokePrivateMethod(form, "ApplyOpen");
                InvokePrivateMethod(form, "ApplyClose");
                InvokePrivateMethod(form, "MeasureCurrentRoi");
                InvokePrivateMethod(form, "ShowHistogram");
            }
            catch (Exception exception)
            {
                noImageCommandException = exception;
            }
        });

        Assert.Null(capturedException);
        Assert.Null(noImageCommandException);
        Assert.Equal("ImageJCsharp", title);
        Assert.Equal("No image", statusText);
        Assert.True(imageCleared);
        Assert.True(imageBoxSizeCleared);
        Assert.True(roiCleared);
        Assert.True(closeDisabledAfterClose);
    }

    [Fact]
    public void Form1ReportsImageSizeAndZoomInStatusBar()
    {
        string? startupStatus = null;
        string? openedStatus = null;
        string? zoomInStatus = null;
        string? zoomOutStatus = null;
        string? actualSizeStatus = null;
        string? fitToWindowStatus = null;

        var capturedException = RunOnStaThread(() =>
        {
            using var form = new Form1();
            startupStatus = GetStatusText(form);

            LoadTestImage(form);
            openedStatus = GetStatusText(form);

            FindMenuItem(form, "View", "Zoom In").PerformClick();
            zoomInStatus = GetStatusText(form);

            FindMenuItem(form, "View", "Zoom Out").PerformClick();
            zoomOutStatus = GetStatusText(form);

            FindMenuItem(form, "View", "Zoom In").PerformClick();
            FindMenuItem(form, "View", "Actual Size").PerformClick();
            actualSizeStatus = GetStatusText(form);

            FindMenuItem(form, "View", "Fit to Window").PerformClick();
            fitToWindowStatus = GetStatusText(form);
        });

        Assert.Null(capturedException);
        Assert.Equal("No image", startupStatus);
        Assert.Equal("2 x 2  Zoom: 100%", openedStatus);
        Assert.Equal("2 x 2  Zoom: 125%", zoomInStatus);
        Assert.Equal("2 x 2  Zoom: 100%", zoomOutStatus);
        Assert.Equal("2 x 2  Zoom: 100%", actualSizeStatus);
        Assert.StartsWith("2 x 2  Zoom: ", fitToWindowStatus);
    }

    [Fact]
    public void MeasureCurrentRoiUsesDocumentCalibration()
    {
        string? area = null;
        string? unit = null;

        var capturedException = RunOnStaThread(() =>
        {
            using var form = new Form1();
            LoadTestImage(form);
            var document = GetPrivateField<ImageDocument>(form, "_document");
            document.Calibration = new PixelCalibration(0.5, 2, "um");

            InvokePrivateMethod(form, "MeasureCurrentRoi");

            var grid = GetPrivateField<DataGridView>(form, "_resultsGrid");
            area = Convert.ToString(grid.Rows[0].Cells["Area"].Value);
            unit = Convert.ToString(grid.Rows[0].Cells["Unit"].Value);
        });

        Assert.Null(capturedException);
        Assert.Equal("1", area);
        Assert.Equal("um", unit);
    }

    [Fact]
    public void MeasureCurrentRoiUsesSelectedMeasurementOptions()
    {
        string[]? columns = null;
        string[]? values = null;

        var capturedException = RunOnStaThread(() =>
        {
            using var form = new Form1();
            LoadTestImage(form);
            SetPrivateField(form, "_measurementOptions", new MeasurementOptions(
                showArea: true,
                showMean: false,
                showMin: false,
                showMax: true,
                showStandardDeviation: false));
            InvokePrivateMethod(form, "ApplyMeasurementOptions");

            InvokePrivateMethod(form, "MeasureCurrentRoi");

            var grid = GetPrivateField<DataGridView>(form, "_resultsGrid");
            columns = grid.Columns.Cast<DataGridViewColumn>().Select(column => column.Name).ToArray();
            values = grid.Rows[0].Cells.Cast<DataGridViewCell>().Select(cell => Convert.ToString(cell.Value) ?? string.Empty).ToArray();
        });

        Assert.Null(capturedException);
        Assert.Equal(new[] { "Name", "Area", "Unit", "Max" }, columns);
        Assert.Equal(new[] { "close-smoke.png", "1", "pixel", "10" }, values);
    }

    [Fact]
    public void RoiManagerCanStoreAndRestoreCurrentRoi()
    {
        int? managedCount = null;
        RectRoi? restoredRoi = null;
        RoiShape? restoredShape = null;

        var capturedException = RunOnStaThread(() =>
        {
            using var form = new Form1();
            LoadTestImage(form);
            SetPrivateField<RectRoi?>(form, "_roi", new RectRoi(2, 3, 4, 5));
            SetPrivateField(form, "_roiShape", RoiShape.Oval);

            InvokePrivateMethod(form, "AddCurrentRoiToManager");
            var managedRois = GetPrivateField<BindingList<ManagedRoi>>(form, "_managedRois");
            managedCount = managedRois.Count;

            SetPrivateField<RectRoi?>(form, "_roi", null);
            SetPrivateField(form, "_roiShape", RoiShape.Rectangle);
            InvokePrivateMethod(form, "SelectManagedRoi", managedRois[0]);

            restoredRoi = GetPrivateField<RectRoi?>(form, "_roi");
            restoredShape = GetPrivateField<RoiShape>(form, "_roiShape");
        });

        Assert.Null(capturedException);
        Assert.Equal(1, managedCount);
        Assert.Equal(RoiShape.Oval, restoredShape);
        Assert.Equal(new RectRoi(2, 3, 4, 5), restoredRoi);
    }

    private static Exception? RunOnStaThread(Action action)
    {
        Exception? capturedException = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                capturedException = exception;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        return capturedException;
    }

    private static ToolStripMenuItem FindMenuItem(Form form, string menuText, string itemText)
    {
        var menu = form.MainMenuStrip?.Items
            .OfType<ToolStripMenuItem>()
            .Single(item => NormalizeMenuText(item.Text) == menuText);

        return menu?.DropDownItems
            .OfType<ToolStripMenuItem>()
            .Single(item => NormalizeMenuText(item.Text) == itemText)
            ?? throw new InvalidOperationException("Menu item was not found.");
    }

    private static string NormalizeMenuText(string text)
    {
        return text.Replace("&", string.Empty);
    }

    private static void LoadTestImage(Form1 form)
    {
        var image = new GrayImage(2, 2);
        image[0, 0] = 10;
        image[1, 0] = 20;
        image[0, 1] = 30;
        image[1, 1] = 40;

        SetPrivateField(form, "_document", new ImageDocument("close-smoke.png", image));
        SetPrivateField<RectRoi?>(form, "_roi", new RectRoi(0, 0, 1, 1));
        InvokePrivateMethod(form, "RefreshDisplay");
        InvokePrivateMethod(form, "UpdateTitle");
        InvokePrivateMethod(form, "UpdateCommandStates");
    }

    private static PictureBox GetImageBox(Form1 form)
    {
        return GetPrivateField<PictureBox>(form, "_imageBox");
    }

    private static string GetStatusText(Form1 form)
    {
        return GetPrivateField<ToolStripStatusLabel>(form, "_statusLabel").Text;
    }

    private static T GetPrivateField<T>(Form1 form, string fieldName)
    {
        var field = typeof(Form1).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not found.");
        return (T)field.GetValue(form)!;
    }

    private static void SetPrivateField<T>(Form1 form, string fieldName, T value)
    {
        var field = typeof(Form1).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not found.");
        field.SetValue(form, value);
    }

    private static void InvokePrivateMethod(Form1 form, string methodName, params object?[] parameters)
    {
        var method = typeof(Form1).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Method '{methodName}' was not found.");
        method.Invoke(form, parameters);
    }
}
