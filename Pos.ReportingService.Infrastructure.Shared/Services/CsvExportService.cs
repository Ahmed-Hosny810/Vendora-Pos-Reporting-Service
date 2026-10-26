
using Microsoft.Extensions.Options;
using Pos.ReportingService.Application.Interfaces.Services;
using Pos.ReportingService.Infrastructure.Shared.Options;
using System.Globalization;
using System.Text;

namespace Pos.ReportingService.Infrastructure.Shared.Services
{
    public class CsvExportService : ICsvExportService
    {
        private static readonly UTF8Encoding Utf8NoBom = new(false);
        private static readonly UTF8Encoding Utf8Bom = new(true);

        private readonly CsvExportOptions _options;

        public CsvExportService(IOptions<CsvExportOptions> options)
            => _options = options.Value;

        public int MaxRows => _options.MaxRows;

        public byte[] Write(
            IReadOnlyList<string> headers,
            IEnumerable<IReadOnlyList<object?>> rows,
            CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();
            Write(stream, headers, rows, cancellationToken);
            return stream.ToArray();
        }

        // Prefer this overload for large exports: no extra byte[] copy.
        public void Write(
            Stream output,
            IReadOnlyList<string> headers,
            IEnumerable<IReadOnlyList<object?>> rows,
            CancellationToken cancellationToken)
        {
            var encoding = _options.IncludeUtf8Bom ? Utf8Bom : Utf8NoBom;

            using var writer = new StreamWriter(output, encoding, 4096, leaveOpen: true)
            {
                NewLine = "\r\n"
            };

            WriteRow(writer, headers);

            var rowCount = 0;
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (++rowCount > MaxRows)
                    throw new InvalidOperationException("The CSV row limit was exceeded.");

                if (row.Count != headers.Count)
                    throw new InvalidOperationException(
                        "The number of CSV values must match the headers.");

                WriteRow(writer, row);
            }

            writer.Flush();
        }

        private void WriteRow<T>(StreamWriter writer, IReadOnlyList<T> values)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (i > 0) writer.Write(_options.Delimiter);
                writer.Write(FormatCell(values[i]));
            }
            writer.WriteLine();
        }

        private static string FormatCell(object? value)
        {
            var text = value switch
            {
                null => string.Empty,
                string s => ProtectSpreadsheetText(s),
                DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
                DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()
            } ?? string.Empty;

            return $"\"{text.Replace("\"", "\"\"")}\"";
        }

        private static string ProtectSpreadsheetText(string text)
        {
            if (text.Length == 0) return text;

            // Excel ignores leading whitespace before '=', so check past it.
            var first = text.AsSpan().TrimStart() is { Length: > 0 } t ? t[0] : text[0];

            return first is '=' or '+' or '-' or '@' or '\t' or '\r' or '\n'
                ? "'" + text
                : text;
        }
    }
}
