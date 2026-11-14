using Pca.Core.Algorithms;
using Pca.Core.Models;
using Pca.Infrastructure.Csv;

namespace Pca.Tests;

public class AlgorithmTests
{
    [Fact]
    public void Jacobi_FindsKnownEigenvalues()
    {
        double[][] m = [[2, 1], [1, 2]];
        var (values, vectors) = new JacobiEigenSolver().Solve(m);

        Assert.Equal(3, values[0], 10);
        Assert.Equal(1, values[1], 10);
        Assert.Equal(1 / Math.Sqrt(2), Math.Abs(vectors[0][0]), 10);
    }

    [Fact]
    public void Pca_OnPerfectLine_ExplainsAllVarianceInFirstComponent()
    {
        var rows = Enumerable.Range(0, 10).Select(i => new[] { (double)i, 2.0 * i }).ToArray();
        var data = new Dataset(["x", "y"], rows, Enumerable.Repeat("", 10).ToArray());

        var result = new PrincipalComponentAnalyzer(new JacobiEigenSolver(), new PcaOptions(Standardize: false)).Fit(data);

        Assert.Equal(1.0, result.ExplainedVarianceRatio[0], 10);
        Assert.Equal(0.0, result.Eigenvalues[1], 10);
    }

    [Fact]
    public void Pca_ScoresAreUncorrelatedAndVarianceMatchesEigenvalues()
    {
        var rng = new Random(1);
        var rows = Enumerable.Range(0, 200).Select(_ =>
        {
            double a = rng.NextDouble(), b = rng.NextDouble();
            return new[] { a, a + 0.1 * b, b, a - b };
        }).ToArray();
        var data = new Dataset(["a", "b", "c", "d"], rows, Enumerable.Repeat("", 200).ToArray());

        var r = new PrincipalComponentAnalyzer(new JacobiEigenSolver(), new PcaOptions(true)).Fit(data);

        double var0 = r.Scores.Sum(s => s[0] * s[0]) / 199;
        double cross = r.Scores.Sum(s => s[0] * s[1]) / 199;
        Assert.Equal(r.Eigenvalues[0], var0, 8);
        Assert.Equal(0, cross, 8);
        Assert.Equal(1.0, r.ExplainedVarianceRatio.Sum(), 10);
    }

    [Fact]
    public void KMeans_SeparatesTwoDistantBlobs()
    {
        double[][] pts = [[0, 0], [0.1, 0], [0, 0.1], [10, 10], [10.1, 10], [10, 10.1]];
        var result = new KMeansClusterer(new KMeansOptions(K: 2)).Cluster(pts);

        Assert.Equal(result.Assignments[0], result.Assignments[1]);
        Assert.Equal(result.Assignments[3], result.Assignments[4]);
        Assert.NotEqual(result.Assignments[0], result.Assignments[3]);
    }

    [Fact]
    public void Elbow_InertiaDecreasesAndDropsSharplyAtTrueK()
    {
        double[][] pts = [[0, 0], [0.1, 0], [0, 0.1], [10, 10], [10.1, 10], [10, 10.1]];
        var elbow = new ElbowAnalyzer(new KMeansOptions(), new ElbowOptions(4)).Analyze(pts);

        Assert.Equal(4, elbow.Count);
        for (int i = 1; i < elbow.Count; i++) Assert.True(elbow[i].Inertia <= elbow[i - 1].Inertia + 1e-9);
        Assert.True(elbow[1].Inertia < 0.01 * elbow[0].Inertia);
    }

    [Fact]
    public async Task CsvReader_ParsesIrisSample()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "iris.csv");
        var data = await new CsvDatasetReader().ReadAsync(path);

        Assert.Equal(150, data.Count);
        Assert.Equal(4, data.Dimensions);
        Assert.Equal(3, data.Labels.Distinct().Count());
    }

    [Fact]
    public async Task CsvReader_RejectsNonNumericFeature()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "a,b,l\n1,x,foo\n");
        await Assert.ThrowsAsync<InvalidDataException>(() => new CsvDatasetReader().ReadAsync(path));
    }
}
