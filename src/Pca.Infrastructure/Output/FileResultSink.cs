using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Pca.Core.Abstractions;
using Pca.Core.Models;

namespace Pca.Infrastructure.Output;

/// <summary>Writes <c>report.txt</c> and <c>pca_scores.csv</c> (scores + cluster per sample) to the output directory.</summary>
public sealed class FileResultSink(ReportFormatter formatter, IOptions<OutputOptions> options) : IResultSink
{
    public async Task WriteAsync(AnalysisResult result, CancellationToken cancellationToken = default)
    {
        var dir = Directory.CreateDirectory(options.Value.Directory).FullName;
        await File.WriteAllTextAsync(Path.Combine(dir, "report.txt"), formatter.Format(result), cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(dir, "pca_scores.csv"), BuildScoresCsv(result), cancellationToken);
    }

    private static string BuildScoresCsv(AnalysisResult r)
    {
        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        int pcs = r.Pca.Components.Length;
        sb.AppendLine("sample,label,cluster," + string.Join(",", Enumerable.Range(1, pcs).Select(k => $"PC{k}")));
        for (int i = 0; i < r.Dataset.Count; i++)
        {
            sb.Append(inv, $"{i + 1},{r.Dataset.Labels[i]},{r.Clustering.Assignments[i]},");
            sb.AppendLine(string.Join(",", r.Pca.Scores[i].Select(s => s.ToString("F6", inv))));
        }
        return sb.ToString();
    }
}
