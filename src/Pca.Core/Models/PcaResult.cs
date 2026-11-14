namespace Pca.Core.Models;

/// <param name="Mean">Per-feature mean used for centering.</param>
/// <param name="Scale">Per-feature divisor (standard deviation, or 1 when not standardizing).</param>
/// <param name="Eigenvalues">Variances along each principal component, descending.</param>
/// <param name="ExplainedVarianceRatio">Eigenvalue share of the total variance.</param>
/// <param name="Components">Loadings: Components[k] is the k-th unit eigenvector over the features.</param>
/// <param name="Scores">Projection of every sample onto all components: Scores[i][k].</param>
public sealed record PcaResult(
    double[] Mean,
    double[] Scale,
    double[] Eigenvalues,
    double[] ExplainedVarianceRatio,
    double[][] Components,
    double[][] Scores);
