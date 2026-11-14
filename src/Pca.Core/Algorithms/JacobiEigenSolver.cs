using Pca.Core.Abstractions;

namespace Pca.Core.Algorithms;

/// <summary>Cyclic Jacobi rotation method; robust and exact enough for small symmetric matrices.</summary>
public sealed class JacobiEigenSolver : IEigenSolver
{
    private const int MaxSweeps = 100;
    private const double Tolerance = 1e-12;

    public (double[] Values, double[][] Vectors) Solve(double[][] symmetricMatrix)
    {
        int n = symmetricMatrix.Length;
        var a = symmetricMatrix.Select(r => (double[])r.Clone()).ToArray();
        var v = new double[n][];
        for (int i = 0; i < n; i++) { v[i] = new double[n]; v[i][i] = 1; }

        for (int sweep = 0; sweep < MaxSweeps && OffDiagonalNorm(a) > Tolerance; sweep++)
        {
            for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                if (Math.Abs(a[p][q]) < 1e-300) continue;
                double theta = (a[q][q] - a[p][p]) / (2 * a[p][q]);
                double t = (theta >= 0 ? 1 : -1) / (Math.Abs(theta) + Math.Sqrt(theta * theta + 1));
                double c = 1 / Math.Sqrt(t * t + 1), s = t * c;

                for (int k = 0; k < n; k++)
                {
                    double akp = a[k][p], akq = a[k][q];
                    a[k][p] = c * akp - s * akq;
                    a[k][q] = s * akp + c * akq;
                }
                for (int k = 0; k < n; k++)
                {
                    double apk = a[p][k], aqk = a[q][k];
                    a[p][k] = c * apk - s * aqk;
                    a[q][k] = s * apk + c * aqk;
                }
                for (int k = 0; k < n; k++)
                {
                    double vkp = v[k][p], vkq = v[k][q];
                    v[k][p] = c * vkp - s * vkq;
                    v[k][q] = s * vkp + c * vkq;
                }
            }
        }

        var order = Enumerable.Range(0, n).OrderByDescending(i => a[i][i]).ToArray();
        var values = order.Select(i => a[i][i]).ToArray();
        // Columns of v are eigenvectors; normalise sign (largest |component| positive) for reproducible output.
        var vectors = order.Select(i =>
        {
            var vec = Enumerable.Range(0, n).Select(k => v[k][i]).ToArray();
            double big = vec.MaxBy(Math.Abs);
            return big < 0 ? vec.Select(x => -x).ToArray() : vec;
        }).ToArray();
        return (values, vectors);
    }

    private static double OffDiagonalNorm(double[][] a)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            for (int j = 0; j < a.Length; j++)
                if (i != j) sum += a[i][j] * a[i][j];
        return Math.Sqrt(sum);
    }
}
