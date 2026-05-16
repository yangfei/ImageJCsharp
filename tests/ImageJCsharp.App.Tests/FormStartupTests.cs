using ImageJCsharp.App;
using System.Windows.Forms;

namespace ImageJCsharp.App.Tests;

public sealed class FormStartupTests
{
    [Fact]
    public void Form1ConstructsWithoutMenuShortcutExceptions()
    {
        Exception? capturedException = null;

        var thread = new Thread(() =>
        {
            try
            {
                using var form = new Form1();
            }
            catch (Exception exception)
            {
                capturedException = exception;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(capturedException);
    }

    [Fact]
    public void Form1IncludesMinimumImageJLikeTopLevelMenus()
    {
        string[]? menuTexts = null;
        Exception? capturedException = null;

        var thread = new Thread(() =>
        {
            try
            {
                using var form = new Form1();
                menuTexts = form.MainMenuStrip?.Items
                    .OfType<ToolStripMenuItem>()
                    .Select(item => item.Text.Replace("&", string.Empty))
                    .ToArray();
            }
            catch (Exception exception)
            {
                capturedException = exception;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(capturedException);
        Assert.Equal(
            new[] { "File", "Edit", "Image", "Process", "Analyze", "View", "Window", "Help" },
            menuTexts);
    }
}
