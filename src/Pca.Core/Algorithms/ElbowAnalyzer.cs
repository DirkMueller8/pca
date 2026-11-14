using Pca.Core.Abstractions;
using Pca.Core.Models;

namespace Pca.Core.Algorithms;

/// <summary>Runs k-means for k = 1..MaxK (capped by the sample count) and records the inertia of each.</summary>
public sealed class ElbowAnalyzer(KMeansOptions baseOptions, ElbowOptions elbowOptions) : IElbowAnalyzer
{
    public IReadOnlyList<ElbowPoint> Analyze(double[][] points)
    {
        int maxK = Math.Min(elbowOptions.MaxK, points.Length);
        return Enumerable.Range(1, maxK)
            .Select(k => new ElbowPoint(k, new KMeansClusterer(baseOptions with { K = k }).Cluster(points).Inertia))
            .ToArray();
    }
}
