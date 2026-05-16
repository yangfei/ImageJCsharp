using System;
using System.Linq;
using System.Text;

namespace ImageJCsharp.App;

public static class ResultsCsvExporter
{
    public static string Export(ResultsTable table)
    {
        if (table is null)
        {
            throw new ArgumentNullException(nameof(table));
        }

        var builder = new StringBuilder();
        AppendRow(builder, table.Headers);

        foreach (var row in table.Rows)
        {
            AppendRow(builder, row);
        }

        return builder.ToString();
    }

    private static void AppendRow(StringBuilder builder, System.Collections.Generic.IEnumerable<string> values)
    {
        builder.AppendLine(string.Join(",", values.Select(Escape)));
    }

    private static string Escape(string? value)
    {
        value ??= string.Empty;
        var mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n");
        if (!mustQuote)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
