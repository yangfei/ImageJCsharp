using ImageJCsharp.App;

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
}
