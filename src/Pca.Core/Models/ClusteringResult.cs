namespace Pca.Core.Models;

/// <param name="Assignments">Cluster index per sample.</param>
/// <param name="Centroids">Cluster centers in the space that was clustered.</param>
/// <param name="Inertia">Sum of squared distances of samples to their centroid.</param>
/// <param name="Iterations">Iterations of the best run.</param>
public sealed record ClusteringResult(
    int[] Assignments,
    double[][] Centroids,
    double Inertia,
    int Iterations);
