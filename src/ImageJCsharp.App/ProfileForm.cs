using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ImageJCsharp.Core;

namespace ImageJCsharp.App;

public sealed class ProfileForm : Form
{
    private readonly ProfileResult _profile;
    private readonly Panel _plotPanel = new Panel();

    public ProfileForm(string imageName, ProfileResult profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        Text = "Plot Profile of " + imageName;
        Width = 640;
        Height = 360;
        BuildUi();
    }

    private void BuildUi()
    {
        _plotPanel.Dock = DockStyle.Fill;
        _plotPanel.BackColor = Color.White;
        _plotPanel.Paint += PlotPanelPaint;
        Controls.Add(_plotPanel);
    }

    private void PlotPanelPaint(object? sender, PaintEventArgs e)
    {
        e.Graphics.Clear(Color.White);
        if (_profile.Values.Length == 0)
        {
            return;
        }

        var bounds = _plotPanel.ClientRectangle;
        bounds.Inflate(-32, -24);
        e.Graphics.DrawRectangle(Pens.LightGray, bounds);

        var max = Math.Max(1, (int)_profile.Values.Max());
        PointF Map(int index, ushort value)
        {
            var x = bounds.Left + (bounds.Width * (index / (float)Math.Max(1, _profile.Values.Length - 1)));
            var y = bounds.Bottom - (bounds.Height * (value / (float)max));
            return new PointF(x, y);
        }

        if (_profile.Values.Length == 1)
        {
            var point = Map(0, _profile.Values[0]);
            e.Graphics.FillEllipse(Brushes.RoyalBlue, point.X - 2, point.Y - 2, 4, 4);
            return;
        }

        var points = _profile.Values
            .Select((value, index) => Map(index, value))
            .ToArray();
        e.Graphics.DrawLines(Pens.RoyalBlue, points);
    }
}
