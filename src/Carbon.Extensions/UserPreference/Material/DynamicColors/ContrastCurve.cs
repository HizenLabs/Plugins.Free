using HizenLabs.Extensions.UserPreference.Material.Utils;

namespace HizenLabs.Extensions.UserPreference.Material.DynamicColors;

/// <summary>
/// Represents a curve defining tone adjustments at different contrast levels.
/// </summary>
public sealed class ContrastCurve
{
    public double Low { get; }
    public double Normal { get; }
    public double Medium { get; }
    public double High { get; }

    private ContrastCurve(double low, double normal, double medium, double high)
    {
        Low = low;
        Normal = normal;
        Medium = medium;
        High = high;
    }

    public static ContrastCurve Create(double low, double normal, double medium, double high)
    {
        return new ContrastCurve(low, normal, medium, high);
    }

    /// <summary>
    /// Gets the tone adjustment for a given contrast level.
    /// </summary>
    public double Get(double contrastLevel)
    {
        if (contrastLevel <= -1.0)
            return Low;
        if (contrastLevel < 0.0)
            return MathUtils.Lerp(Low, Normal, (contrastLevel + 1.0));
        if (contrastLevel < 0.5)
            return MathUtils.Lerp(Normal, Medium, contrastLevel / 0.5);
        if (contrastLevel < 1.0)
            return MathUtils.Lerp(Medium, High, (contrastLevel - 0.5) / 0.5);
        return High;
    }
}
