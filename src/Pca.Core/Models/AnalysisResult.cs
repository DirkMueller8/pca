namespace Pca.Core.Models;

/// <summary>Everything produced by one analysis run; consumed by result sinks.</summary>
public sealed record AnalysisResult(
    Dataset Dataset,
    PcaResult Pca,
    ClusteringResult Clustering,
    IReadOnlyList<ElbowPoint> Elbow);

/// <summary>Best k-means inertia for a given number of clusters (one point of the elbow curve).</summary>
public sealed record ElbowPoint(int K, double Inertia);
