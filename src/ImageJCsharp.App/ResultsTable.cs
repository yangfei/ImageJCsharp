using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageJCsharp.App;

public sealed class ResultsTable
{
    public ResultsTable(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows)
    {
        if (headers is null)
        {
            throw new ArgumentNullException(nameof(headers));
        }

        if (rows is null)
        {
            throw new ArgumentNullException(nameof(rows));
        }

        Headers = headers.ToArray();
        Rows = rows.Select(row => row?.ToArray() ?? Array.Empty<string>()).ToArray();
    }

    public IReadOnlyList<string> Headers { get; }

    public IReadOnlyList<IReadOnlyList<string>> Rows { get; }
}
