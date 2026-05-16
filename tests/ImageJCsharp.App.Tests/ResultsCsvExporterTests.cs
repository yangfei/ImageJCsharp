using ImageJCsharp.App;

namespace ImageJCsharp.App.Tests;

public sealed class ResultsCsvExporterTests
{
    [Fact]
    public void ExportIncludesHeadersAndRows()
    {
        var table = new ResultsTable(
            new[] { "Name", "Area", "Mean" },
            new[]
            {
                new[] { "cell.png", "12.5", "42" },
                new[] { "field.png", "3", "7.25" }
            });

        var csv = ResultsCsvExporter.Export(table);

        Assert.Equal(
            "Name,Area,Mean\r\ncell.png,12.5,42\r\nfield.png,3,7.25\r\n",
            csv);
    }

    [Fact]
    public void ExportEscapesCommasQuotesAndNewLines()
    {
        var table = new ResultsTable(
            new[] { "Name", "Note" },
            new[]
            {
                new[] { "cell, one", "quoted \"value\"" },
                new[] { "line", "first\r\nsecond" }
            });

        var csv = ResultsCsvExporter.Export(table);

        Assert.Equal(
            "Name,Note\r\n\"cell, one\",\"quoted \"\"value\"\"\"\r\nline,\"first\r\nsecond\"\r\n",
            csv);
    }

    [Fact]
    public void ExportEmptyTableWritesOnlyHeaders()
    {
        var table = new ResultsTable(new[] { "Name", "Area" }, Array.Empty<string[]>());

        var csv = ResultsCsvExporter.Export(table);

        Assert.Equal("Name,Area\r\n", csv);
    }
}
