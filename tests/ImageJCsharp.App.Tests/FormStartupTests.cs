using ImageJCsharp.App;
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
}
