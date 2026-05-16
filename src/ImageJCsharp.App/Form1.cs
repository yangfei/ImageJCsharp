using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ImageJCsharp.Core;

namespace ImageJCsharp.App;

public partial class Form1 : Form
{
    private readonly PictureBox _imageBox = new PictureBox();
    private readonly Panel _imagePanel = new Panel();
    private readonly DataGridView _resultsGrid = new DataGridView();
    private readonly StatusStrip _statusStrip = new StatusStrip();
    private readonly ToolStripStatusLabel _statusLabel = new ToolStripStatusLabel();
    private readonly List<ToolStripMenuItem> _requiresActiveImageItems = new List<ToolStripMenuItem>();
    private readonly List<ToolStripMenuItem> _requiresResultsItems = new List<ToolStripMenuItem>();
    private readonly BindingList<ManagedRoi> _managedRois = new BindingList<ManagedRoi>();
    private const string NoImageStatusText = "No image";
    private MeasurementOptions _measurementOptions = MeasurementOptions.Default;
    private DisplayAdjustment? _displayAdjustment;
    private ImageDocument? _document;
    private Bitmap? _displayBitmap;
    private RectRoi? _roi;
    private LineRoi? _lineRoi;
    private RoiShape _roiShape = RoiShape.Rectangle;
    private RoiShape _selectionTool = RoiShape.Rectangle;
    private Point? _dragStartImagePoint;
    private RectRoi? _resizeStartRoi;
    private RoiResizeHandle _activeResizeHandle = RoiResizeHandle.None;
    private double _zoom = 1d;

    public Form1()
    {
        InitializeComponent();
        FormClosed += (_, _) => _displayBitmap?.Dispose();
        BuildUi();
        UpdateTitle();
    }

    private void BuildUi()
    {
        Text = "ImageJCsharp";
        Width = 1100;
        Height = 760;

        var menu = new MenuStrip();
        MainMenuStrip = menu;
        Controls.Add(menu);

        var file = AddMenu(menu, "&File");
        AddItem(file, "&Open...", OpenImage, Keys.Control | Keys.O);
        AddActiveImageItem(file, "&Save As...", SaveImageAs, Keys.Control | Keys.S);
        file.DropDownItems.Add(new ToolStripSeparator());
        AddActiveImageItem(file, "&Close", CloseImage, Keys.Control | Keys.W);
        AddItem(file, "E&xit", () => Close(), Keys.Alt | Keys.F4);

        var edit = AddMenu(menu, "&Edit");
        AddItem(edit, "&Rectangle Selection", () => _selectionTool = RoiShape.Rectangle);
        AddItem(edit, "&Oval Selection", () => _selectionTool = RoiShape.Oval);
        AddItem(edit, "&Line Selection", () => _selectionTool = RoiShape.Line);

        var image = AddMenu(menu, "&Image");
        AddActiveImageItem(image, "&Brightness/Contrast...", AdjustBrightnessContrast);
        AddActiveImageItem(image, "&Reset Display", ResetDisplayAdjustment);

        var process = AddMenu(menu, "&Process");
        AddActiveImageItem(process, "&Invert", ApplyInvert);
        AddActiveImageItem(process, "&Find Edges", ApplyFindEdges);
        AddActiveImageItem(process, "&Threshold...", ApplyThreshold);

        var analyze = AddMenu(menu, "&Analyze");
        AddActiveImageItem(analyze, "&Measure", MeasureCurrentRoi, Keys.Control | Keys.M);
        AddActiveImageItem(analyze, "&Histogram", ShowHistogram, shortcutKeyDisplayString: "H");
        AddActiveImageItem(analyze, "Plot &Profile", ShowProfile);
        AddActiveImageItem(analyze, "Set &Scale...", SetScale);
        AddItem(analyze, "Set &Measurements...", SetMeasurements);
        AddActiveImageItem(analyze, "Add ROI to &Manager", AddCurrentRoiToManager);
        AddResultsItem(analyze, "Export &Results...", ExportResults, Keys.Control | Keys.E);

        var view = AddMenu(menu, "&View");
        AddActiveImageItem(view, "Zoom &In", () => ChangeZoom(1.25), Keys.Control | Keys.Add);
        AddActiveImageItem(view, "Zoom &Out", () => ChangeZoom(0.8), Keys.Control | Keys.Subtract);
        AddActiveImageItem(view, "&Actual Size", () => SetZoom(1), Keys.Control | Keys.D0);
        AddActiveImageItem(view, "&Fit to Window", FitToWindow);

        var window = AddMenu(menu, "&Window");
        AddItem(window, "&ROI Manager", ShowRoiManager);

        var help = AddMenu(menu, "&Help");
        AddDisabledItem(help, "No Help commands yet");

        _imagePanel.Dock = DockStyle.Fill;
        _imagePanel.AutoScroll = true;
        _imagePanel.BackColor = Color.FromArgb(44, 44, 44);
        Controls.Add(_imagePanel);

        _imageBox.SizeMode = PictureBoxSizeMode.StretchImage;
        _imageBox.BackColor = Color.Black;
        _imageBox.Paint += ImageBoxPaint;
        _imageBox.MouseDown += ImageBoxMouseDown;
        _imageBox.MouseMove += ImageBoxMouseMove;
        _imageBox.MouseUp += ImageBoxMouseUp;
        _imagePanel.Controls.Add(_imageBox);

        _resultsGrid.Dock = DockStyle.Bottom;
        _resultsGrid.Height = 150;
        _resultsGrid.AllowUserToAddRows = false;
        _resultsGrid.ReadOnly = true;
        _resultsGrid.RowHeadersVisible = false;
        ApplyMeasurementOptions();
        Controls.Add(_resultsGrid);

        _statusStrip.Items.Add(_statusLabel);
        Controls.Add(_statusStrip);

        _statusLabel.Text = NoImageStatusText;
        UpdateCommandStates();
    }

    private static ToolStripMenuItem AddMenu(MenuStrip menu, string text)
    {
        var item = new ToolStripMenuItem(text);
        menu.Items.Add(item);
        return item;
    }

    private static ToolStripMenuItem AddItem(
        ToolStripMenuItem menu,
        string text,
        Action action,
        Keys shortcut = Keys.None,
        string shortcutKeyDisplayString = "")
    {
        var item = new ToolStripMenuItem(text);
        if (shortcut != Keys.None)
        {
            item.ShortcutKeys = shortcut;
        }

        if (!string.IsNullOrEmpty(shortcutKeyDisplayString))
        {
            item.ShortcutKeyDisplayString = shortcutKeyDisplayString;
        }

        item.Click += (_, _) => action();
        menu.DropDownItems.Add(item);
        return item;
    }

    private void AddActiveImageItem(
        ToolStripMenuItem menu,
        string text,
        Action action,
        Keys shortcut = Keys.None,
        string shortcutKeyDisplayString = "")
    {
        _requiresActiveImageItems.Add(AddItem(menu, text, action, shortcut, shortcutKeyDisplayString));
    }

    private void AddResultsItem(
        ToolStripMenuItem menu,
        string text,
        Action action,
        Keys shortcut = Keys.None,
        string shortcutKeyDisplayString = "")
    {
        _requiresResultsItems.Add(AddItem(menu, text, action, shortcut, shortcutKeyDisplayString));
    }

    private static void AddDisabledItem(ToolStripMenuItem menu, string text)
    {
        var item = new ToolStripMenuItem(text)
        {
            Enabled = false
        };
        menu.DropDownItems.Add(item);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.H)
        {
            ShowHistogram();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void OpenImage()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All files|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        using var bitmap = new Bitmap(dialog.FileName);
        _document = new ImageDocument(dialog.FileName, BitmapConversion.ToGrayImage(bitmap));
        _roi = null;
        _lineRoi = null;
        _managedRois.Clear();
        _roiShape = RoiShape.Rectangle;
        _displayAdjustment = null;
        _zoom = 1d;
        RefreshDisplay();
        UpdateTitle();
        UpdateCommandStates();
    }

    private void SaveImageAs()
    {
        if (_document is null)
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp|TIFF|*.tif",
            FileName = Path.GetFileNameWithoutExtension(_document.DisplayName) + ".png"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        using var bitmap = BitmapConversion.ToBitmap(_document.Image);
        bitmap.Save(dialog.FileName, GetImageFormat(dialog.FileName));
        _statusLabel.Text = "Saved " + dialog.FileName;
    }

    private void CloseImage()
    {
        _document = null;
        _roi = null;
        _lineRoi = null;
        _managedRois.Clear();
        _roiShape = RoiShape.Rectangle;
        _displayAdjustment = null;
        _displayBitmap?.Dispose();
        _displayBitmap = null;
        _imageBox.Image = null;
        _imageBox.Size = Size.Empty;
        _imageBox.Invalidate();
        _statusLabel.Text = NoImageStatusText;
        _resizeStartRoi = null;
        _activeResizeHandle = RoiResizeHandle.None;
        UpdateTitle();
        UpdateCommandStates();
    }

    private void ApplyInvert()
    {
        if (_document is null)
        {
            return;
        }

        _document.Image = ImageProcessor.Invert(_document.Image, 255);
        RefreshDisplay();
    }

    private void ApplyFindEdges()
    {
        if (_document is null)
        {
            return;
        }

        _document.Image = ImageProcessor.SobelEdges(_document.Image);
        RefreshDisplay();
    }

    private void ApplyThreshold()
    {
        if (_document is null)
        {
            return;
        }

        var minimumText = Prompt("Minimum threshold", "128");
        if (!ushort.TryParse(minimumText, out var minimum))
        {
            return;
        }

        _document.Image = BinaryImageConversion.ToGrayImage(ImageProcessor.Threshold(_document.Image, minimum, 255));
        RefreshDisplay();
    }

    private void MeasureCurrentRoi()
    {
        if (_document is null)
        {
            return;
        }

        if (_lineRoi is not null)
        {
            MessageBox.Show(this, "Measure currently supports full image, rectangle ROI, and oval ROI selections. Use Plot Profile for line ROI intensity values.", "Unsupported ROI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var fullImageRoi = new RectRoi(0, 0, _document.Image.Width, _document.Image.Height);
        var result = _roi is null || _roiShape == RoiShape.Rectangle
            ? Measurements.Measure(_document.Image, _roi ?? fullImageRoi, _document.Calibration)
            : Measurements.Measure(_document.Image, new OvalRoi(_roi.Value.X, _roi.Value.Y, _roi.Value.Width, _roi.Value.Height), _document.Calibration);
        _resultsGrid.Rows.Add(CreateMeasurementRow(result));
        UpdateCommandStates();
    }

    private string[] CreateMeasurementRow(MeasurementResult result)
    {
        var values = new List<string> { _document?.DisplayName ?? string.Empty };
        if (_measurementOptions.ShowArea)
        {
            values.Add(result.Area.ToString("0.###"));
            values.Add(_document?.Calibration.Unit ?? PixelCalibration.Identity.Unit);
        }

        if (_measurementOptions.ShowMean)
        {
            values.Add(result.Mean.ToString("0.###"));
        }

        if (_measurementOptions.ShowMin)
        {
            values.Add(result.Min.ToString("0.###"));
        }

        if (_measurementOptions.ShowMax)
        {
            values.Add(result.Max.ToString("0.###"));
        }

        if (_measurementOptions.ShowStandardDeviation)
        {
            values.Add(result.StandardDeviation.ToString("0.###"));
        }

        return values.ToArray();
    }

    private void SetMeasurements()
    {
        using var form = new Form
        {
            Text = "Set Measurements",
            Width = 260,
            Height = 250,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false
        };

        var list = new CheckedListBox
        {
            Dock = DockStyle.Top,
            Height = 150,
            CheckOnClick = true
        };
        list.Items.Add("Area", _measurementOptions.ShowArea);
        list.Items.Add("Mean", _measurementOptions.ShowMean);
        list.Items.Add("Min", _measurementOptions.ShowMin);
        list.Items.Add("Max", _measurementOptions.ShowMax);
        list.Items.Add("StdDev", _measurementOptions.ShowStandardDeviation);
        form.Controls.Add(list);

        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Dock = DockStyle.Bottom };
        form.Controls.Add(cancel);
        form.Controls.Add(ok);
        form.AcceptButton = ok;
        form.CancelButton = cancel;

        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _measurementOptions = new MeasurementOptions(
            list.GetItemChecked(0),
            list.GetItemChecked(1),
            list.GetItemChecked(2),
            list.GetItemChecked(3),
            list.GetItemChecked(4));
        ApplyMeasurementOptions();
        UpdateCommandStates();
    }

    private void ApplyMeasurementOptions()
    {
        _resultsGrid.Rows.Clear();
        _resultsGrid.Columns.Clear();
        foreach (var column in _measurementOptions.GetColumnNames())
        {
            _resultsGrid.Columns.Add(column, column);
        }
    }

    private void SetScale()
    {
        if (_document is null)
        {
            return;
        }

        var widthText = Prompt("Pixel width", _document.Calibration.PixelWidth.ToString("0.###"));
        if (!double.TryParse(widthText, out var pixelWidth) || pixelWidth <= 0)
        {
            return;
        }

        var heightText = Prompt("Pixel height", _document.Calibration.PixelHeight.ToString("0.###"));
        if (!double.TryParse(heightText, out var pixelHeight) || pixelHeight <= 0)
        {
            return;
        }

        var unit = Prompt("Unit", _document.Calibration.Unit);
        if (string.IsNullOrWhiteSpace(unit))
        {
            return;
        }

        _document.Calibration = new PixelCalibration(pixelWidth, pixelHeight, unit.Trim());
        _statusLabel.Text = $"Scale: {pixelWidth:0.###} x {pixelHeight:0.###} {unit.Trim()}/pixel";
    }

    private void AddCurrentRoiToManager()
    {
        if (_lineRoi is not null)
        {
            _managedRois.Add(ManagedRoi.FromLine(_lineRoi.Value));
            return;
        }

        if (_roi is null)
        {
            return;
        }

        _managedRois.Add(_roiShape == RoiShape.Oval
            ? ManagedRoi.Oval(_roi.Value)
            : ManagedRoi.Rectangle(_roi.Value));
    }

    private void ShowRoiManager()
    {
        var form = new RoiManagerForm(_managedRois, SelectManagedRoi, DeleteManagedRoi);
        form.Show(this);
    }

    private void SelectManagedRoi(ManagedRoi roi)
    {
        if (roi.Shape == RoiShape.Line)
        {
            _lineRoi = roi.Line;
            _roi = null;
            _roiShape = RoiShape.Line;
        }
        else
        {
            _lineRoi = null;
            _roi = roi.Bounds;
            _roiShape = roi.Shape;
        }

        _imageBox.Invalidate();
    }

    private void DeleteManagedRoi(ManagedRoi roi)
    {
        _managedRois.Remove(roi);
    }

    private void UpdateCommandStates()
    {
        var hasActiveImage = _document is not null;
        foreach (var item in _requiresActiveImageItems)
        {
            item.Enabled = hasActiveImage;
        }

        var hasResults = _resultsGrid.Rows.Cast<DataGridViewRow>().Any(row => !row.IsNewRow);
        foreach (var item in _requiresResultsItems)
        {
            item.Enabled = hasResults;
        }
    }

    private void ShowHistogram()
    {
        if (_document is null)
        {
            return;
        }

        if (_lineRoi is not null)
        {
            MessageBox.Show(this, "Histogram currently supports full image or rectangle ROI selections.", "Unsupported ROI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var histogram = _roi is null
            ? Histogram.Calculate(_document.Image)
            : _roiShape == RoiShape.Rectangle
                ? Histogram.Calculate(_document.Image, _roi.Value)
                : null;

        if (histogram is null)
        {
            MessageBox.Show(this, "Histogram currently supports full image or rectangle ROI selections.", "Unsupported ROI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var form = new HistogramForm(_document.DisplayName, histogram);
        form.Show(this);
    }

    private void ShowProfile()
    {
        if (_document is null)
        {
            return;
        }

        var profile = _lineRoi is not null
            ? Profile.Line(_document.Image, _lineRoi.Value)
            : _roi is null
                ? Profile.HorizontalCenterLine(_document.Image)
                : _roiShape == RoiShape.Rectangle
                    ? Profile.HorizontalCenterLine(_document.Image, _roi.Value)
                    : null;

        if (profile is null)
        {
            MessageBox.Show(this, "Plot Profile currently supports full image or rectangle ROI selections.", "Unsupported ROI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var form = new ProfileForm(_document.DisplayName, profile);
        form.Show(this);
    }

    private void ExportResults()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "CSV files|*.csv|All files|*.*",
            FileName = "results.csv"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var table = CreateResultsTable();
        File.WriteAllText(dialog.FileName, ResultsCsvExporter.Export(table));
        _statusLabel.Text = "Exported results to " + dialog.FileName;
    }

    private ResultsTable CreateResultsTable()
    {
        var headers = _resultsGrid.Columns
            .Cast<DataGridViewColumn>()
            .Select(column => column.HeaderText);

        var rows = _resultsGrid.Rows
            .Cast<DataGridViewRow>()
            .Where(row => !row.IsNewRow)
            .Select(row => _resultsGrid.Columns
                .Cast<DataGridViewColumn>()
                .Select(column => Convert.ToString(row.Cells[column.Index].Value) ?? string.Empty));

        return new ResultsTable(headers, rows);
    }

    private void RefreshDisplay()
    {
        if (_document is null)
        {
            return;
        }

        _displayBitmap?.Dispose();
        _displayBitmap = _displayAdjustment is null
            ? BitmapConversion.ToBitmap(_document.Image)
            : BitmapConversion.ToBitmap(_document.Image, _displayAdjustment.Value);
        _imageBox.Image = _displayBitmap;
        ResizeImageBox();
        _imageBox.Invalidate();
        _statusLabel.Text = $"{_document.Image.Width} x {_document.Image.Height}  Zoom: {_zoom:P0}";
    }

    private void ResizeImageBox()
    {
        if (_document is null)
        {
            return;
        }

        _imageBox.Width = Math.Max(1, (int)Math.Round(_document.Image.Width * _zoom));
        _imageBox.Height = Math.Max(1, (int)Math.Round(_document.Image.Height * _zoom));
    }

    private void ChangeZoom(double factor)
    {
        if (_document is null)
        {
            return;
        }

        SetZoom(_zoom * factor);
    }

    private void SetZoom(double zoom)
    {
        _zoom = Math.Max(0.05, Math.Min(32, zoom));
        ResizeImageBox();
        _imageBox.Invalidate();
        _statusLabel.Text = _document is null ? NoImageStatusText : $"{_document.Image.Width} x {_document.Image.Height}  Zoom: {_zoom:P0}";
    }

    private void FitToWindow()
    {
        if (_document is null || _imagePanel.ClientSize.Width <= 0 || _imagePanel.ClientSize.Height <= 0)
        {
            return;
        }

        var zoomX = (double)_imagePanel.ClientSize.Width / _document.Image.Width;
        var zoomY = (double)_imagePanel.ClientSize.Height / _document.Image.Height;
        SetZoom(Math.Min(zoomX, zoomY));
    }

    private void AdjustBrightnessContrast()
    {
        if (_document is null)
        {
            return;
        }

        var currentMinimum = _displayAdjustment?.Minimum ?? (ushort)0;
        var currentMaximum = _displayAdjustment?.Maximum ?? (ushort)255;
        var minimumText = Prompt("Display minimum", currentMinimum.ToString());
        if (!ushort.TryParse(minimumText, out var minimum))
        {
            return;
        }

        var maximumText = Prompt("Display maximum", currentMaximum.ToString());
        if (!ushort.TryParse(maximumText, out var maximum) || minimum >= maximum)
        {
            return;
        }

        _displayAdjustment = new DisplayAdjustment(minimum, maximum);
        RefreshDisplay();
    }

    private void ResetDisplayAdjustment()
    {
        if (_document is null)
        {
            return;
        }

        _displayAdjustment = null;
        RefreshDisplay();
    }

    private void ImageBoxMouseDown(object? sender, MouseEventArgs e)
    {
        if (_document is null || e.Button != MouseButtons.Left)
        {
            return;
        }

        _dragStartImagePoint = ToImagePoint(e.Location);
        if (_selectionTool == RoiShape.Line)
        {
            _roi = null;
            _roiShape = RoiShape.Line;
            _lineRoi = new LineRoi(_dragStartImagePoint.Value.X, _dragStartImagePoint.Value.Y, _dragStartImagePoint.Value.X, _dragStartImagePoint.Value.Y);
            _imageBox.Invalidate();
            return;
        }

        _activeResizeHandle = _roi is null
            ? RoiResizeHandle.None
            : RoiResizeInteraction.HitTestHandle(_roi.Value, e.Location, _zoom);

        if (_activeResizeHandle == RoiResizeHandle.None)
        {
            _resizeStartRoi = null;
            _lineRoi = null;
            _roiShape = _selectionTool;
            _roi = new RectRoi(_dragStartImagePoint.Value.X, _dragStartImagePoint.Value.Y, 1, 1);
        }
        else
        {
            _resizeStartRoi = _roi;
        }

        _imageBox.Invalidate();
    }

    private void ImageBoxMouseMove(object? sender, MouseEventArgs e)
    {
        if (_document is null)
        {
            return;
        }

        if (_dragStartImagePoint is not null)
        {
            if (_lineRoi is not null)
            {
                var end = ToImagePoint(e.Location);
                _lineRoi = new LineRoi(_dragStartImagePoint.Value.X, _dragStartImagePoint.Value.Y, end.X, end.Y);
            }
            else if (_activeResizeHandle == RoiResizeHandle.None)
            {
                _roi = CreateRoi(_dragStartImagePoint.Value, ToImagePoint(e.Location), _document.Image.Width, _document.Image.Height);
            }
            else if (_resizeStartRoi is not null)
            {
                _roi = RoiResizeInteraction.Resize(
                    _resizeStartRoi.Value,
                    _activeResizeHandle,
                    ToImagePoint(e.Location),
                    _document.Image.Width,
                    _document.Image.Height);
            }

            _imageBox.Invalidate();
            return;
        }

        UpdateImageBoxCursor(e.Location);
    }

    private void ImageBoxMouseUp(object? sender, MouseEventArgs e)
    {
        _dragStartImagePoint = null;
        _resizeStartRoi = null;
        _activeResizeHandle = RoiResizeHandle.None;
        UpdateImageBoxCursor(e.Location);
    }

    private void ImageBoxPaint(object? sender, PaintEventArgs e)
    {
        using var pen = new Pen(Color.Yellow, 1);
        if (_lineRoi is not null)
        {
            e.Graphics.DrawLine(
                pen,
                (float)(_lineRoi.Value.X1 * _zoom),
                (float)(_lineRoi.Value.Y1 * _zoom),
                (float)(_lineRoi.Value.X2 * _zoom),
                (float)(_lineRoi.Value.Y2 * _zoom));
            return;
        }

        if (_roi is null)
        {
            return;
        }

        var roi = _roi.Value;
        var bounds = new RectangleF(
            (float)(roi.X * _zoom),
            (float)(roi.Y * _zoom),
            (float)(roi.Width * _zoom),
            (float)(roi.Height * _zoom));
        if (_roiShape == RoiShape.Oval)
        {
            e.Graphics.DrawEllipse(pen, bounds);
        }
        else
        {
            e.Graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        DrawRoiHandles(e.Graphics, roi);
    }

    private void DrawRoiHandles(Graphics graphics, RectRoi roi)
    {
        foreach (var handle in new[]
        {
            RoiResizeHandle.NorthWest,
            RoiResizeHandle.North,
            RoiResizeHandle.NorthEast,
            RoiResizeHandle.East,
            RoiResizeHandle.SouthEast,
            RoiResizeHandle.South,
            RoiResizeHandle.SouthWest,
            RoiResizeHandle.West
        })
        {
            var bounds = RoiResizeInteraction.GetHandleBounds(roi, handle, _zoom);
            graphics.FillRectangle(Brushes.White, bounds);
            graphics.DrawRectangle(Pens.Black, bounds);
        }
    }

    private void UpdateImageBoxCursor(Point controlPoint)
    {
        if (_selectionTool == RoiShape.Line || _roi is null)
        {
            _imageBox.Cursor = Cursors.Cross;
            return;
        }

        var handle = RoiResizeInteraction.HitTestHandle(_roi.Value, controlPoint, _zoom);
        _imageBox.Cursor = handle switch
        {
            RoiResizeHandle.NorthWest or RoiResizeHandle.SouthEast => Cursors.SizeNWSE,
            RoiResizeHandle.NorthEast or RoiResizeHandle.SouthWest => Cursors.SizeNESW,
            RoiResizeHandle.North or RoiResizeHandle.South => Cursors.SizeNS,
            RoiResizeHandle.East or RoiResizeHandle.West => Cursors.SizeWE,
            _ => Cursors.Cross
        };
    }

    private Point ToImagePoint(Point controlPoint)
    {
        if (_document is null)
        {
            return Point.Empty;
        }

        var x = Math.Max(0, Math.Min(_document.Image.Width - 1, (int)Math.Floor(controlPoint.X / _zoom)));
        var y = Math.Max(0, Math.Min(_document.Image.Height - 1, (int)Math.Floor(controlPoint.Y / _zoom)));
        return new Point(x, y);
    }

    private static RectRoi CreateRoi(Point start, Point end, int imageWidth, int imageHeight)
    {
        var x = Math.Max(0, Math.Min(start.X, end.X));
        var y = Math.Max(0, Math.Min(start.Y, end.Y));
        var right = Math.Min(imageWidth, Math.Max(start.X, end.X) + 1);
        var bottom = Math.Min(imageHeight, Math.Max(start.Y, end.Y) + 1);
        return new RectRoi(x, y, Math.Max(1, right - x), Math.Max(1, bottom - y));
    }

    private void UpdateTitle()
    {
        Text = _document is null ? "ImageJCsharp" : $"{_document.DisplayName} - ImageJCsharp";
    }

    private static ImageFormat GetImageFormat(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            ".tif" or ".tiff" => ImageFormat.Tiff,
            _ => ImageFormat.Png
        };
    }

    private static string Prompt(string title, string defaultValue)
    {
        using var form = new Form();
        using var label = new Label();
        using var textBox = new TextBox();
        using var ok = new Button();
        using var cancel = new Button();

        form.Text = title;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterParent;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.ClientSize = new Size(260, 105);

        label.Text = title + ":";
        label.SetBounds(12, 12, 230, 20);
        textBox.Text = defaultValue;
        textBox.SetBounds(12, 36, 230, 24);
        ok.Text = "OK";
        ok.DialogResult = DialogResult.OK;
        ok.SetBounds(86, 70, 75, 24);
        cancel.Text = "Cancel";
        cancel.DialogResult = DialogResult.Cancel;
        cancel.SetBounds(167, 70, 75, 24);

        form.Controls.AddRange(new Control[] { label, textBox, ok, cancel });
        form.AcceptButton = ok;
        form.CancelButton = cancel;

        return form.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
    }
}
