# PCA & k-means on the Iris dataset (C# / .NET 10)

A small, clean-code demonstration of **Principal Component Analysis (PCA)** on the classic Iris dataset
(150 samples, 4 features), extended with **k-means clustering**. Input is a CSV file; output is numerical
(console + files) and graphical (2-D PNG plots).

## Goal

- Show how PCA reduces 4-dimensional data to a 2-D plane while keeping most of the variance (~95.8 % with two components on standardized data).
- Show that k-means clustering recovers the species structure and visualize it in the PCA plane.
- Serve as a reference for SOLID design, dependency injection and testable numerical code in C#.

## Quick start

```powershell
dotnet run --project src/Pca.App                                   # uses samples/iris.csv
dotnet run --project src/Pca.App -- --Analysis:InputPath=my.csv --Analysis:Clusters=4 --Analysis:Standardize=false
dotnet test
```

Results land in `results/`:

| File | Content |
|---|---|
| `report.txt` (and console) | eigenvalues, explained variance, loadings, k-means inertia, cluster vs. species table, purity |
| `pca_scores.csv` | per-sample PC scores and cluster id |
| `pca_by_species.png` | PC1/PC2 scatter colored by species |
| `pca_kmeans.png` | PC1/PC2 scatter colored by k-means cluster, with centroids |
| `pca_scree.png` | explained variance per component + cumulative |
| `kmeans_elbow.png` | elbow plot: k-means inertia for k = 1..`Analysis:ElbowMaxK` (default 10), chosen k highlighted |

### CSV format

Header row, numeric feature columns, and an optional trailing text column used as the label (see `samples/iris.csv`).

## Architecture

```
src/
  Pca.Core            domain + algorithms, no external dependencies
    Models/             Dataset, PcaResult, ClusteringResult, options
    Abstractions/       IDatasetReader, IEigenSolver, IPrincipalComponentAnalyzer, IClusterer, IResultSink
    Algorithms/         JacobiEigenSolver, PrincipalComponentAnalyzer, KMeansClusterer, ElbowAnalyzer
    Services/           AnalysisPipeline (use case orchestration)
  Pca.Infrastructure  I/O adapters: CsvDatasetReader, report/CSV sinks, ScottPlot PlotSink
  Pca.App             composition root (DI container, configuration, appsettings.json)
tests/Pca.Tests       xUnit tests
docs/                 further documentation
```

Dependencies point inward: `App -> Infrastructure -> Core`.

SOLID in practice:

- **S** – each class has one job (parsing, eigen solving, PCA, clustering, formatting, plotting).
- **O** – new outputs are new `IResultSink` implementations registered in DI; nothing existing changes.
- **L / I** – small, focused interfaces; any implementation can replace another (e.g. a different `IEigenSolver`).
- **D** – high-level code (`AnalysisPipeline`) depends only on abstractions; `Program.cs` wires concrete types.

Flow: `IDatasetReader` → `IPrincipalComponentAnalyzer` → `IClusterer` (on the full PC scores, which preserve distances) → all `IResultSink`s.

More detail: [docs/architecture.md](docs/architecture.md), [docs/pca-and-kmeans.md](docs/pca-and-kmeans.md).

## Tooling

- .NET 10 / C# (latest), solution file `Pca.slnx`
- `Microsoft.Extensions.Hosting` – dependency injection and configuration
- [ScottPlot 5](https://scottplot.net) – plotting to PNG
- xUnit – tests
- PCA, Jacobi eigen solver and k-means (k-means++ seeding) are implemented from scratch, without a math library.

Dataset: Fisher's Iris data, UCI Machine Learning Repository.
