# PCA and k-means in this project

## PCA

1. Center each feature (subtract the mean); optionally divide by the standard deviation (`Standardize`).
2. Compute the covariance matrix `C` (correlation matrix when standardized).
3. Eigen-decompose `C` (cyclic Jacobi method). Eigenvalues = variance along each component;
   eigenvectors = loadings.
4. Scores = centered data projected on the eigenvectors. The first two columns are plotted.

Explained variance of component *k* = λₖ / Σλ.

For standardized Iris: PC1 72.77 %, PC2 23.03 % (cumulative 95.80 %).

Eigenvector signs are arbitrary; the solver normalizes them so the largest component is positive.

## k-means

Lloyd's algorithm with k-means++ seeding, several restarts, lowest inertia wins, fixed seed.
It clusters the scores of **all** components (distance-preserving rotation of the scaled data), and the
result is displayed in the PC1/PC2 plane. With k = 3, setosa is separated perfectly; versicolor and
virginica overlap, giving ~83 % purity against the species labels.

## Elbow method

`ElbowAnalyzer` runs k-means for k = 1..10 and plots the inertia (within-cluster sum of squares). The
"elbow" is where extra clusters stop paying off. For standardized Iris, the inertia falls 62.7 % from
k=1 to 2 and a further 37.0 % at k=3, after which the drops are much smaller, so k=3 (matching the three species) is a
reasonable choice; k=2 is also defensible because versicolor and virginica overlap. The elbow is a heuristic, not a proof.

## Output files

See the table in the [README](../README.md#quick-start).
