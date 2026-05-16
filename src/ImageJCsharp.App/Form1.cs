using System;
using System.Collections.Generic;
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
    private ImageDocument? _document;
    private Bitmap? _displayBitmap;
    private RectRoi? _roi;
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
        AddDisabledItem(edit, "No Edit commands yet");

        var image = AddMenu(menu, "&Image");
        AddDisabledItem(image, "No Image commands yet");

        var process = AddMenu(menu, "&Process");
        AddActiveImageItem(process, "&Invert", ApplyInvert);
        AddActiveImageItem(process, "&Find Edges", ApplyFindEdges);
        AddActiveImageItem(process, "&Threshold...", ApplyThreshold);

        var analyze = AddMenu(menu, "&Analyze");
        AddActiveImageItem(analyze, "&Measure", MeasureCurrentRoi, Keys.Control | Keys.M);
        AddActiveImageItem(analyze, "&Histogram", ShowHistogram, shortcutKeyDisplayString: "H");
        AddResultsItem(analyze, "Export &Results...", ExportResults, Keys.Control | Keys.E);

        var view = AddMenu(menu, "&View");
        AddActiveImageItem(view, "Zoom &In", () => ChangeZoom(1.25), Keys.Control | Keys.Add);
        AddActiveImageItem(view, "Zoom &Out", () => ChangeZoom(0.8), Keys.Control | Keys.Subtract);
        AddActiveImageItem(view, "&Actual Size", () => SetZoom(1), Keys.Control | Keys.D0);
        AddActiveImageItem(view, "&Fit to Window", FitToWindow);

        var window = AddMenu(menu, "&Window");
        AddDisabledItem(window, "No Window commands yet");

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
        _resultsGrid.Columns.Add("Name", "Name");
        _resultsGrid.Columns.Add("Area", "Area");
        _resultsGrid.Columns.Add("Mean", "Mean");
        _resultsGrid.Columns.Add("Min", "Min");
        _resultsGrid.Columns.Add("Max", "Max");
        _resultsGrid.Columns.Add("StdDev", "StdDev");
        Controls.Add(_resultsGrid);

        _statusStrip.Items.Add(_statusLabel);
        Controls.Add(_statusStrip);

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
        _displayBitmap?.Dispose();
        _displayBitmap = null;
        _imageBox.Image = null;
        _imageBox.Size = Size.Empty;
        _imageBox.Invalidate();
        _statusLabel.Text = string.Empty;
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

        var binary = ImageProcessor.Threshold(_document.Image, minimum, 255);
        var image = new GrayImage(binary.Width, binary.Height);
        for (var y = 0; y < binary.Height; y++)
        {
            for (var x = 0; x < binary.Width; x++)
            {
                image[x, y] = binary[x, y] ? (ushort)255 : (ushort)0;
            }
        }

        _document.Image = image;
        RefreshDisplay();
    }

    private void MeasureCurrentRoi()
    {
        if (_document is null)
        {
            return;
        }

        var roi = _roi ?? new RectRoi(0, 0, _document.Image.Width, _document.Image.Height);
        var result = Measurements.Measure(_document.Image, roi, PixelCalibration.Identity);
        _resultsGrid.Rows.Add(
            _document.DisplayName,
            result.Area.ToString("0.###"),
            result.Mean.ToString("0.###"),
            result.Min.ToString("0.###"),
            result.Max.ToString("0.###"),
            result.StandardDeviation.ToString("0.###"));
        UpdateCommandStates();
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

        var histogram = _roi is null
            ? Histogram.Calculate(_document.Image)
            : Histogram.Calculate(_document.Image, _roi.Value);

        var form = new HistogramForm(_document.DisplayName, histogram);
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
        _displayBitmap = BitmapConversion.ToBitmap(_document.Image);
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
        _statusLabel.Text = _document is null ? string.Empty : $"{_document.Image.Width} x {_document.Image.Height}  Zoom: {_zoom:P0}";
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

    private void ImageBoxMouseDown(object? sender, MouseEventArgs e)
    {
        if (_document is null || e.Button != MouseButtons.Left)
        {
            return;
        }

        _dragStartImagePoint = ToImagePoint(e.Location);
        _activeResizeHandle = _roi is null
            ? RoiResizeHandle.None
            : RoiResizeInteraction.HitTestHandle(_roi.Value, e.Location, _zoom);

        if (_activeResizeHandle == RoiResizeHandle.None)
        {
            _resizeStartRoi = null;
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
            if (_activeResizeHandle == RoiResizeHandle.None)
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
        if (_roi is null)
        {
            return;
        }

        var roi = _roi.Value;
        using var pen = new Pen(Color.Yellow, 1);
        e.Graphics.DrawRectangle(
            pen,
            (float)(roi.X * _zoom),
            (float)(roi.Y * _zoom),
            (float)(roi.Width * _zoom),
            (float)(roi.Height * _zoom));

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
        if (_roi is null)
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
