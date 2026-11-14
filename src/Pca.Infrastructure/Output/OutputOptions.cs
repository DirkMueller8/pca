namespace Pca.Infrastructure.Output;

public sealed class OutputOptions
{
    public const string Section = "Output";

    public string Directory { get; set; } = "results";
    public int PlotWidth { get; set; } = 900;
    public int PlotHeight { get; set; } = 700;
}
