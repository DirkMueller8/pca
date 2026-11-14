using System.Globalization;
using System.Text;
using Pca.Core.Models;

namespace Pca.Infrastructure.Output;

/// <summary>Turns an <see cref="AnalysisResult"/> into a human readable numerical report.</summary>
public sealed class ReportFormatter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public string Format(AnalysisResult r)
    {
        var sb = new StringBuilder();
        var pca = r.Pca;
        var names = r.Dataset.FeatureNames;

        sb.AppendLine($"Samples: {r.Dataset.Count}, features: {r.Dataset.Dimensions}");
        sb.AppendLine();
        sb.AppendLine("== PCA: explained variance ==");
        sb.AppendLine("Component  Eigenvalue  Explained  Cumulative");
        double cumulative = 0;
        for (int k = 0; k < pca.Eigenvalues.Length; k++)
        {
            cumulative += pca.ExplainedVarianceRatio[k];
            sb.AppendLine(string.Create(Inv, $"PC{k + 1,-8} {pca.Eigenvalues[k],10:F4}  {pca.ExplainedVarianceRatio[k],8:P2}  {cumulative,9:P2}"));
        }

        sb.AppendLine();
        sb.AppendLine("== PCA: loadings (eigenvectors) ==");
        sb.Append($"{"Feature",-14}");
        for (int k = 0; k < pca.Components.Length; k++) sb.Append($"{"PC" + (k + 1),9}");
        sb.AppendLine();
        for (int j = 0; j < names.Count; j++)
        {
            sb.Append($"{names[j],-14}");
            for (int k = 0; k < pca.Components.Length; k++)
                sb.Append(string.Create(Inv, $"{pca.Components[k][j],9:F4}"));
            sb.AppendLine();
        }

        var c = r.Clustering;
        sb.AppendLine();
        sb.AppendLine($"== K-means (k={c.Centroids.Length}) ==");
        sb.AppendLine(string.Create(Inv, $"Inertia: {c.Inertia:F4}, iterations (best run): {c.Iterations}"));
        sb.AppendLine("Cluster sizes: " + string.Join(", ",
            Enumerable.Range(0, c.Centroids.Length).Select(i => $"#{i}={c.Assignments.Count(a => a == i)}")));

        sb.AppendLine();
        sb.AppendLine("== Elbow method (inertia per k) ==");
        for (int i = 0; i < r.Elbow.Count; i++)
        {
            double drop = i == 0 ? 0 : 1 - r.Elbow[i].Inertia / r.Elbow[i - 1].Inertia;
            sb.AppendLine(string.Create(Inv, $"k={r.Elbow[i].K,-3} inertia={r.Elbow[i].Inertia,10:F4}  drop vs. k-1: {drop,6:P1}"));
        }

        if (r.Dataset.Labels.Any(l => l.Length > 0))
        {
            AppendContingency(sb, r);
        }
        return sb.ToString();
    }

    private static void AppendContingency(StringBuilder sb, AnalysisResult r)
    {
        var labels = r.Dataset.Labels.Distinct().Order().ToArray();
        int k = r.Clustering.Centroids.Length;
        sb.AppendLine();
        sb.AppendLine("Cluster vs. known label (counts):");
        sb.Append($"{"",-18}");
        for (int i = 0; i < k; i++) sb.Append($"{"#" + i,6}");
        sb.AppendLine();

        int purityHits = 0;
        var table = labels.Select(l => Enumerable.Range(0, k)
            .Select(i => r.Dataset.Labels.Where((lab, idx) => lab == l && r.Clustering.Assignments[idx] == i).Count()).ToArray()).ToArray();
        for (int row = 0; row < labels.Length; row++)
            sb.AppendLine($"{labels[row],-18}" + string.Concat(table[row].Select(n => $"{n,6}")));
        for (int col = 0; col < k; col++) purityHits += table.Max(row => row[col]);
        sb.AppendLine(string.Create(Inv, $"Purity: {(double)purityHits / r.Dataset.Count:P1}"));
    }
}
