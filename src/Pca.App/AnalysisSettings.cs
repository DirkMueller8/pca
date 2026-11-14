namespace Pca.App;

public sealed class AnalysisSettings
{
    public const string Section = "Analysis";

    public string InputPath { get; set; } = "samples/iris.csv";
    public bool Standardize { get; set; } = true;
    public int Clusters { get; set; } = 3;
    public int Seed { get; set; } = 42;
    public int ElbowMaxK { get; set; } = 10;
}
