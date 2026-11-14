namespace Pca.Core.Models;

public sealed record PcaOptions(bool Standardize = true);

public sealed record ElbowOptions(int MaxK = 10);

public sealed record KMeansOptions(int K = 3, int MaxIterations = 300, int Restarts = 10, int Seed = 42);
