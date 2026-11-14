using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pca.App;
using Pca.Core.Abstractions;
using Pca.Core.Algorithms;
using Pca.Core.Models;
using Pca.Core.Services;
using Pca.Infrastructure.Csv;
using Pca.Infrastructure.Output;
using Pca.Infrastructure.Plotting;

// Composition root: the only place that knows concrete types.
// Configuration: appsettings.json, overridable on the command line, e.g.  --Analysis:InputPath=my.csv --Analysis:Clusters=4
// Content root = executable folder so appsettings.json is found regardless of the working directory;
// relative data/output paths still resolve against the working directory.
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.Configure<AnalysisSettings>(builder.Configuration.GetSection(AnalysisSettings.Section));
builder.Services.Configure<OutputOptions>(builder.Configuration.GetSection(OutputOptions.Section));

var analysis = builder.Configuration.GetSection(AnalysisSettings.Section).Get<AnalysisSettings>() ?? new AnalysisSettings();
builder.Services.AddSingleton(new PcaOptions(analysis.Standardize));
builder.Services.AddSingleton(new KMeansOptions(analysis.Clusters, Seed: analysis.Seed));
builder.Services.AddSingleton(new ElbowOptions(analysis.ElbowMaxK));
builder.Services.AddSingleton<IElbowAnalyzer, ElbowAnalyzer>();

builder.Services.AddSingleton<IDatasetReader, CsvDatasetReader>();
builder.Services.AddSingleton<IEigenSolver, JacobiEigenSolver>();
builder.Services.AddSingleton<IPrincipalComponentAnalyzer, PrincipalComponentAnalyzer>();
builder.Services.AddSingleton<IClusterer, KMeansClusterer>();
builder.Services.AddSingleton<ReportFormatter>();
builder.Services.AddSingleton<IResultSink, ConsoleReportSink>();
builder.Services.AddSingleton<IResultSink, FileResultSink>();
builder.Services.AddSingleton<IResultSink, PlotSink>();
builder.Services.AddSingleton<AnalysisPipeline>();

using var host = builder.Build();
var pipeline = host.Services.GetRequiredService<AnalysisPipeline>();

try
{
    await pipeline.RunAsync(analysis.InputPath);
    var outDir = Path.GetFullPath(builder.Configuration[$"{OutputOptions.Section}:{nameof(OutputOptions.Directory)}"] ?? "results");
    Console.WriteLine($"Results written to {outDir}");
    return 0;
}
catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
