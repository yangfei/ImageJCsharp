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
        MinimumSize = new Size(360, 280);
        BuildUi();
        UpdateStats();
    }

    private void BuildUi()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        Controls.Add(layout);

        _plotPanel.Dock = DockStyle.Fill;
        _plotPanel.BackColor = Color.White;
        _plotPanel.Paint += PlotPanelPaint;
        _plotPanel.MouseMove += PlotPanelMouseMove;
        _plotPanel.MouseLeave += (_, _) => _hoverLabel.Text = "Value: -, Count: -";
        layout.Controls.Add(_plotPanel, 0, 0);

        _statsLabel.Dock = DockStyle.Fill;
        _statsLabel.Padding = new Padding(8, 6, 8, 0);
        layout.Controls.Add(_statsLabel, 0, 1);

        _hoverLabel.Dock = DockStyle.Fill;
        _hoverLabel.Padding = new Padding(8, 4, 8, 0);
        _hoverLabel.Text = "Value: -, Count: -";
        layout.Controls.Add(_hoverLabel, 0, 2);
    }

    private void UpdateStats()
    {
        _statsLabel.Text =
            $"Count: {_histogram.Count}    Mean: {_histogram.Mean:0.###}    StdDev: {_histogram.StandardDeviation:0.###}\r\n" +
            $"Min: {_histogram.Min}    Max: {_histogram.Max}    Mode: {_histogram.Mode}";
    }

    private void PlotPanelPaint(object? sender, PaintEventArgs e)
    {
        var plotBounds = GetPlotBounds();
        if (plotBounds.Width <= 0 || plotBounds.Height <= 0)
        {
            return;
        }

        var lutBounds = GetLutBounds(plotBounds);
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
        var plotBounds = GetPlotBounds();
        if (!plotBounds.Contains(e.Location))
        {
            _hoverLabel.Text = "Value: -, Count: -";
            return;
        }

        var value = Math.Max(0, Math.Min(255, (int)Math.Floor((double)(e.X - plotBounds.Left) * 256 / plotBounds.Width)));
        _hoverLabel.Text = $"Value: {value}, Count: {_histogram.Bins[value]}";
    }

    private Rectangle GetPlotBounds()
    {
        var bounds = _plotPanel.ClientRectangle;
        return new Rectangle(36, 12, Math.Max(1, bounds.Width - 52), Math.Max(1, bounds.Height - 44));
    }

    private static Rectangle GetLutBounds(Rectangle plotBounds)
    {
        return new Rectangle(plotBounds.Left, plotBounds.Bottom + 8, plotBounds.Width, 12);
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
