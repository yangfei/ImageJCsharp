using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ImageJCsharp.App;

public sealed class RoiManagerForm : Form
{
    private readonly BindingList<ManagedRoi> _rois;
    private readonly Action<ManagedRoi> _selectRoi;
    private readonly Action<ManagedRoi> _deleteRoi;
    private readonly ListBox _listBox = new ListBox();

    public RoiManagerForm(BindingList<ManagedRoi> rois, Action<ManagedRoi> selectRoi, Action<ManagedRoi> deleteRoi)
    {
        _rois = rois ?? throw new ArgumentNullException(nameof(rois));
        _selectRoi = selectRoi ?? throw new ArgumentNullException(nameof(selectRoi));
        _deleteRoi = deleteRoi ?? throw new ArgumentNullException(nameof(deleteRoi));

        Text = "ROI Manager";
        Width = 320;
        Height = 360;
        BuildUi();
    }

    private void BuildUi()
    {
        _listBox.Dock = DockStyle.Fill;
        _listBox.DataSource = _rois;
        Controls.Add(_listBox);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight
        };

        var select = new Button { Text = "Select", Width = 80 };
        select.Click += (_, _) =>
        {
            if (_listBox.SelectedItem is ManagedRoi roi)
            {
                _selectRoi(roi);
            }
        };

        var delete = new Button { Text = "Delete", Width = 80 };
        delete.Click += (_, _) =>
        {
            if (_listBox.SelectedItem is ManagedRoi roi)
            {
                _deleteRoi(roi);
            }
        };

        var close = new Button { Text = "Close", Width = 80 };
        close.Click += (_, _) => Close();

        buttons.Controls.Add(select);
        buttons.Controls.Add(delete);
        buttons.Controls.Add(close);
        Controls.Add(buttons);
    }
}
