namespace Pca.Core.Models;

/// <summary>Tabular numeric data with optional class labels (e.g. the Iris species).</summary>
public sealed record Dataset(
    IReadOnlyList<string> FeatureNames,
    double[][] Rows,
    IReadOnlyList<string> Labels)
{
    public int Count => Rows.Length;
    public int Dimensions => FeatureNames.Count;
}
