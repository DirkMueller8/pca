using Pca.Core.Abstractions;
using Pca.Core.Models;

namespace Pca.Core.Algorithms;

/// <summary>Lloyd's k-means with k-means++ seeding and multiple restarts (lowest inertia wins). Deterministic via seed.</summary>
public sealed class KMeansClusterer(KMeansOptions options) : IClusterer
{
    public ClusteringResult Cluster(double[][] points)
    {
        if (options.K < 1 || options.K > points.Length)
            throw new ArgumentOutOfRangeException(nameof(options), "K must be between 1 and the number of points.");

        var rng = new Random(options.Seed);
        ClusteringResult? best = null;
        for (int r = 0; r < Math.Max(1, options.Restarts); r++)
        {
            var run = RunOnce(points, rng);
            if (best is null || run.Inertia < best.Inertia) best = run;
        }
        return best!;
    }

    private ClusteringResult RunOnce(double[][] points, Random rng)
    {
        var centroids = SeedPlusPlus(points, rng);
        var assignments = new int[points.Length];
        Array.Fill(assignments, -1);
        int iter = 0;
        for (; iter < options.MaxIterations; iter++)
        {
            bool changed = false;
            for (int i = 0; i < points.Length; i++)
            {
                int c = Nearest(points[i], centroids);
                if (c != assignments[i]) { assignments[i] = c; changed = true; }
            }
            if (!changed) break;
            centroids = Recompute(points, assignments, centroids);
        }
        double inertia = points.Select((p, i) => SquaredDistance(p, centroids[assignments[i]])).Sum();
        return new ClusteringResult(assignments, centroids, inertia, iter);
    }

    private double[][] SeedPlusPlus(double[][] points, Random rng)
    {
        var centroids = new List<double[]> { (double[])points[rng.Next(points.Length)].Clone() };
        while (centroids.Count < options.K)
        {
            var d2 = points.Select(p => centroids.Min(c => SquaredDistance(p, c))).ToArray();
            double pick = rng.NextDouble() * d2.Sum(), acc = 0;
            int chosen = points.Length - 1;
            for (int i = 0; i < d2.Length; i++)
            {
                acc += d2[i];
                if (acc >= pick) { chosen = i; break; }
            }
            centroids.Add((double[])points[chosen].Clone());
        }
        return [.. centroids];
    }

    private double[][] Recompute(double[][] points, int[] assignments, double[][] previous)
    {
        int d = points[0].Length;
        var sums = Enumerable.Range(0, options.K).Select(_ => new double[d]).ToArray();
        var counts = new int[options.K];
        for (int i = 0; i < points.Length; i++)
        {
            counts[assignments[i]]++;
            for (int j = 0; j < d; j++) sums[assignments[i]][j] += points[i][j];
        }
        for (int c = 0; c < options.K; c++)
        {
            if (counts[c] == 0) { sums[c] = previous[c]; continue; } // keep empty clusters in place
            for (int j = 0; j < d; j++) sums[c][j] /= counts[c];
        }
        return sums;
    }

    private static int Nearest(double[] p, double[][] centroids)
    {
        int best = 0;
        double bestD = double.MaxValue;
        for (int c = 0; c < centroids.Length; c++)
        {
            double dist = SquaredDistance(p, centroids[c]);
            if (dist < bestD) { bestD = dist; best = c; }
        }
        return best;
    }

    private static double SquaredDistance(double[] a, double[] b)
    {
        double s = 0;
        for (int i = 0; i < a.Length; i++) { double t = a[i] - b[i]; s += t * t; }
        return s;
    }
}
