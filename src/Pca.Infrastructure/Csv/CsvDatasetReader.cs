using System.Globalization;
using Pca.Core.Abstractions;
using Pca.Core.Models;

namespace Pca.Infrastructure.Csv;

/// <summary>
/// Reads a comma separated file with a header row. Numeric columns become features;
/// a trailing non-numeric column (e.g. species) becomes the label.
/// </summary>
public sealed class CsvDatasetReader : IDatasetReader
{
    public async Task<Dataset> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var lines = (await File.ReadAllLinesAsync(path, cancellationToken))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();
        if (lines.Length < 2) throw new InvalidDataException($"'{path}' needs a header and at least one data row.");

        var header = Split(lines[0]);
        var rows = new List<double[]>();
        var labels = new List<string>();
        int featureCount = header.Length;
        bool hasLabel = false;

        for (int i = 1; i < lines.Length; i++)
        {
            var cells = Split(lines[i]);
            if (cells.Length != header.Length)
                throw new InvalidDataException($"Line {i + 1}: expected {header.Length} columns, found {cells.Length}.");

            hasLabel = !double.TryParse(cells[^1], NumberStyles.Float, CultureInfo.InvariantCulture, out _);
            featureCount = hasLabel ? header.Length - 1 : header.Length;

            var values = new double[featureCount];
            for (int j = 0; j < featureCount; j++)
            {
                if (!double.TryParse(cells[j], NumberStyles.Float, CultureInfo.InvariantCulture, out values[j]))
                    throw new InvalidDataException($"Line {i + 1}, column '{header[j]}': '{cells[j]}' is not a number.");
            }
            rows.Add(values);
            labels.Add(hasLabel ? cells[^1] : string.Empty);
        }

        return new Dataset(header.Take(featureCount).ToArray(), [.. rows], labels);
    }

    private static string[] Split(string line) =>
        line.Split(',').Select(c => c.Trim().Trim('"')).ToArray();
}
