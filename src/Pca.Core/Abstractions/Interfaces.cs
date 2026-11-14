using Pca.Core.Models;

namespace Pca.Core.Abstractions;

public interface IDatasetReader
{
    Task<Dataset> ReadAsync(string path, CancellationToken cancellationToken = default);
}

public interface IEigenSolver
{
    /// <summary>Eigen-decomposition of a symmetric matrix. Values descending; Vectors[k] pairs with Values[k].</summary>
    (double[] Values, double[][] Vectors) Solve(double[][] symmetricMatrix);
}

public interface IPrincipalComponentAnalyzer
{
    PcaResult Fit(Dataset dataset);
}

public interface IClusterer
{
    ClusteringResult Cluster(double[][] points);
}

public interface IElbowAnalyzer
{
    /// <summary>Inertia of the best k-means run for k = 1..maxK, used to pick k via the elbow method.</summary>
    IReadOnlyList<ElbowPoint> Analyze(double[][] points);
}

/// <summary>An output channel (console, CSV, plots...). Add new sinks without touching existing code.</summary>
public interface IResultSink
{
    Task WriteAsync(AnalysisResult result, CancellationToken cancellationToken = default);
}
