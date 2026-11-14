using Microsoft.Extensions.Options;
using Pca.Core.Abstractions;
using Pca.Core.Models;
using Pca.Infrastructure.Output;
using ScottPlot;

namespace Pca.Infrastructure.Plotting;

/// <summary>Renders the 2-D PCA projections and scree plot as PNG files using ScottPlot.</summary>
public sealed class PlotSink(IOptions<OutputOptions> options) : IResultSink
{
    // Okabe-Ito colour-blind safe palette.
    private static readonly string[] Palette = ["#0072B2", "#E69F00", "#009E73", "#CC79A7", "#D55E00", "#56B4E9", "#F0E442"];

    public Task WriteAsync(AnalysisResult result, CancellationToken cancellationToken = default)
    {
        var o = options.Value;
        var dir = Directory.CreateDirectory(o.Directory).FullName;

        var labels = result.Dataset.Labels.Select(l => l.Length > 0 ? l : "samples").ToArray();
        Save(ScatterByGroup(result, labels, "PCA projection by species"), Path.Combine(dir, "pca_by_species.png"), o);

        var clusters = result.Clustering.Assignments.Select(a => $"Cluster {a}").ToArray();
        var kmeans = ScatterByGroup(result, clusters, "K-means clusters in PCA plane");
        AddCentroids(kmeans, result);
        Save(kmeans, Path.Combine(dir, "pca_kmeans.png"), o);

        Save(Scree(result.Pca), Path.Combine(dir, "pca_scree.png"), o);
        Save(Elbow(result.Elbow, result.Clustering.Centroids.Length), Path.Combine(dir, "kmeans_elbow.png"), o);
        return Task.CompletedTask;
    }

    private static Plot ScatterByGroup(AnalysisResult r, string[] groups, string title)
    {
        var plot = new Plot();
        var distinct = groups.Distinct().Order().ToArray();
        for (int g = 0; g < distinct.Length; g++)
        {
            var idx = Enumerable.Range(0, groups.Length).Where(i => groups[i] == distinct[g]).ToArray();
            var sp = plot.Add.ScatterPoints(
                idx.Select(i => r.Pca.Scores[i][0]).ToArray(),
                idx.Select(i => r.Pca.Scores[i][1]).ToArray());
            sp.LegendText = distinct[g];
            sp.Color = Color.FromHex(Palette[g % Palette.Length]);
            sp.MarkerSize = 9;
        }
        plot.Title(title);
        plot.XLabel(AxisLabel(r.Pca, 0));
        plot.YLabel(AxisLabel(r.Pca, 1));
        plot.ShowLegend(Alignment.UpperRight);
        return plot;
    }

    private static void AddCentroids(Plot plot, AnalysisResult r)
    {
        var xs = r.Clustering.Centroids.Select(c => c[0]).ToArray();
        var ys = r.Clustering.Centroids.Select(c => c[1]).ToArray();
        var sp = plot.Add.ScatterPoints(xs, ys);
        sp.LegendText = "Centroids";
        sp.Color = Colors.Black;
        sp.MarkerShape = MarkerShape.Cross;
        sp.MarkerSize = 18;
        sp.MarkerLineWidth = 3;
    }

    private static Plot Scree(PcaResult pca)
    {
        var plot = new Plot();
        var bars = pca.ExplainedVarianceRatio
            .Select((v, i) => new Bar { Position = i + 1, Value = v * 100, FillColor = Color.FromHex(Palette[0]) })
            .ToArray();
        plot.Add.Bars(bars);

        double cum = 0;
        var cumulative = pca.ExplainedVarianceRatio.Select(v => cum += v * 100).ToArray();
        var line = plot.Add.Scatter(Enumerable.Range(1, cumulative.Length).Select(i => (double)i).ToArray(), cumulative);
        line.Color = Color.FromHex(Palette[3]);
        line.LegendText = "Cumulative";

        plot.Axes.Bottom.SetTicks(
            Enumerable.Range(1, bars.Length).Select(i => (double)i).ToArray(),
            Enumerable.Range(1, bars.Length).Select(i => $"PC{i}").ToArray());
        plot.Axes.SetLimitsY(0, 105);
        plot.Title("Scree plot");
        plot.YLabel("Explained variance (%)");
        plot.ShowLegend(Alignment.MiddleRight);
        return plot;
    }

    private static Plot Elbow(IReadOnlyList<ElbowPoint> elbow, int chosenK)
    {
        var plot = new Plot();
        var line = plot.Add.Scatter(elbow.Select(e => (double)e.K).ToArray(), elbow.Select(e => e.Inertia).ToArray());
        line.Color = Color.FromHex(Palette[0]);
        line.LineWidth = 2;
        line.MarkerSize = 9;
        line.LegendText = "Inertia";

        var chosen = elbow.FirstOrDefault(e => e.K == chosenK);
        if (chosen is not null)
        {
            var mark = plot.Add.ScatterPoints(new[] { (double)chosen.K }, new[] { chosen.Inertia });
            mark.Color = Color.FromHex(Palette[4]);
            mark.MarkerSize = 16;
            mark.MarkerShape = MarkerShape.OpenCircle;
            mark.MarkerLineWidth = 3;
            mark.LegendText = $"Chosen k = {chosenK}";
        }

        plot.Axes.Bottom.SetTicks(
            elbow.Select(e => (double)e.K).ToArray(),
            elbow.Select(e => e.K.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray());
        plot.Axes.SetLimitsY(0, elbow.Max(e => e.Inertia) * 1.05);
        plot.Title("Elbow plot: k-means inertia vs. k");
        plot.XLabel("Number of clusters k");
        plot.YLabel("Inertia (within-cluster sum of squares)");
        plot.ShowLegend(Alignment.UpperRight);
        return plot;
    }

    private static string AxisLabel(PcaResult pca, int k) =>
        FormattableString.Invariant($"PC{k + 1} ({pca.ExplainedVarianceRatio[k]:P1})");

    private static void Save(Plot plot, string path, OutputOptions o) =>
        plot.SavePng(path, o.PlotWidth, o.PlotHeight);
}
