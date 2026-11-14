using Pca.Core.Abstractions;
using Pca.Core.Models;

namespace Pca.Core.Algorithms;

/// <summary>PCA via eigen-decomposition of the covariance (or correlation, when standardizing) matrix.</summary>
public sealed class PrincipalComponentAnalyzer(IEigenSolver eigenSolver, PcaOptions options) : IPrincipalComponentAnalyzer
{
    public PcaResult Fit(Dataset dataset)
    {
        int n = dataset.Count, d = dataset.Dimensions;
        if (n < 2) throw new ArgumentException("At least two samples are required.", nameof(dataset));

        var mean = new double[d];
        foreach (var row in dataset.Rows)
            for (int j = 0; j < d; j++) mean[j] += row[j] / n;

        var scale = Enumerable.Repeat(1.0, d).ToArray();
        if (options.Standardize)
        {
            for (int j = 0; j < d; j++)
            {
                double ss = dataset.Rows.Sum(r => (r[j] - mean[j]) * (r[j] - mean[j]));
                scale[j] = Math.Sqrt(ss / (n - 1));
                if (scale[j] == 0) scale[j] = 1;
            }
        }

        var centered = dataset.Rows
            .Select(r => Enumerable.Range(0, d).Select(j => (r[j] - mean[j]) / scale[j]).ToArray())
            .ToArray();

        var cov = new double[d][];
        for (int a = 0; a < d; a++)
        {
            cov[a] = new double[d];
            for (int b = 0; b < d; b++)
                cov[a][b] = centered.Sum(r => r[a] * r[b]) / (n - 1);
        }

        var (values, vectors) = eigenSolver.Solve(cov);
        double total = values.Sum();
        var scores = centered
            .Select(r => vectors.Select(v => v.Zip(r, (x, y) => x * y).Sum()).ToArray())
            .ToArray();

        return new PcaResult(mean, scale, values, values.Select(v => v / total).ToArray(), vectors, scores);
    }
}
