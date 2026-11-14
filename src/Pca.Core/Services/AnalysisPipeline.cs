using Pca.Core.Abstractions;
using Pca.Core.Models;

namespace Pca.Core.Services;

/// <summary>Application use case: read -> PCA -> k-means -> publish to all sinks.</summary>
public sealed class AnalysisPipeline(
    IDatasetReader reader,
    IPrincipalComponentAnalyzer pca,
    IClusterer clusterer,
    IElbowAnalyzer elbow,
    IEnumerable<IResultSink> sinks)
{
    public async Task<AnalysisResult> RunAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        var dataset = await reader.ReadAsync(inputPath, cancellationToken);
        var pcaResult = pca.Fit(dataset);
        // Scores over all components preserve Euclidean distances of the (scaled) data, so clustering them
        // equals clustering the original feature space; the 2-D plot is just a projection of that result.
        var clustering = clusterer.Cluster(pcaResult.Scores);
        var result = new AnalysisResult(dataset, pcaResult, clustering, elbow.Analyze(pcaResult.Scores));

        foreach (var sink in sinks)
            await sink.WriteAsync(result, cancellationToken);
        return result;
    }
}
