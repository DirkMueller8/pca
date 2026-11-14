using Pca.Core.Abstractions;
using Pca.Core.Models;

namespace Pca.Infrastructure.Output;

public sealed class ConsoleReportSink(ReportFormatter formatter) : IResultSink
{
    public Task WriteAsync(AnalysisResult result, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(formatter.Format(result));
        return Task.CompletedTask;
    }
}
