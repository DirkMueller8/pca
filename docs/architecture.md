# Architecture

## Layers

| Project | Responsibility | References |
|---|---|---|
| `Pca.Core` | Models, algorithm implementations, interfaces, pipeline | – |
| `Pca.Infrastructure` | CSV reading, text/CSV output, PNG plotting | Core, ScottPlot |
| `Pca.App` | Composition root, configuration | Core, Infrastructure |
| `Pca.Tests` | Unit tests | Core, Infrastructure |

## Dependency injection

`Program.cs` registers everything with `Microsoft.Extensions.DependencyInjection`. `IResultSink` has
several registrations; `AnalysisPipeline` receives them as `IEnumerable<IResultSink>`.

## Configuration (`appsettings.json`, overridable by command line)

| Key | Default | Meaning |
|---|---|---|
| `Analysis:InputPath` | `samples/iris.csv` | input CSV |
| `Analysis:Standardize` | `true` | PCA on correlation (true) or covariance (false) matrix |
| `Analysis:Clusters` | `3` | k for k-means |
| `Analysis:Seed` | `42` | RNG seed for reproducible clustering |
| `Analysis:ElbowMaxK` | `10` | largest k evaluated for the elbow plot |
| `Output:Directory` | `results` | output folder |
| `Output:PlotWidth/PlotHeight` | `900/700` | PNG size |

## Extending

- New output (e.g. HTML): implement `IResultSink`, register it.
- New input format: implement `IDatasetReader`.
- Other clustering: implement `IClusterer`.
- Other eigen solver (e.g. MathNet): implement `IEigenSolver`.
