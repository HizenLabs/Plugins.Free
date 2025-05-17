using HizenLabs.Extensions.UserPreference.Material.Enums;

namespace HizenLabs.Extensions.UserPreference.Material.DynamicColors;

/// <summary>
/// Defines a required tone difference between two dynamic colors.
/// </summary>
public sealed class ToneDeltaPair
{
    public DynamicColor RoleA { get; }
    public DynamicColor RoleB { get; }
    public double Delta { get; }
    public TonePolarity Polarity { get; }
    public bool StayTogether { get; }
    public DeltaConstraint Constraint { get; }

    private ToneDeltaPair(
        DynamicColor roleA,
        DynamicColor roleB,
        double delta,
        TonePolarity polarity,
        bool stayTogether)
    {
        RoleA = roleA;
        RoleB = roleB;
        Delta = delta;
        Polarity = polarity;
        StayTogether = stayTogether;
        Constraint = DeltaConstraint.Exact;
    }

    private ToneDeltaPair(
        DynamicColor roleA,
        DynamicColor roleB,
        double delta,
        TonePolarity polarity,
        DeltaConstraint constraint)
    {
        RoleA = roleA;
        RoleB = roleB;
        Delta = delta;
        Polarity = polarity;
        StayTogether = true;
        Constraint = constraint;
    }

    public static ToneDeltaPair Create(
        DynamicColor roleA,
        DynamicColor roleB,
        double delta,
        TonePolarity polarity,
        bool stayTogether)
    {
        return new(roleA, roleB, delta, polarity, stayTogether);
    }

    public static ToneDeltaPair Create(
        DynamicColor roleA,
        DynamicColor roleB,
        double delta,
        TonePolarity polarity,
        DeltaConstraint constraint)
    {
        return new(roleA, roleB, delta, polarity, constraint);
    }
}
