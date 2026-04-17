using Sandbox;

namespace DarkRp;

/// <summary>
/// Pure data — describes one job. No game logic lives here.
/// Add new jobs in JobRegistry, not here.
/// </summary>
public sealed class JobDefinition
{
    public string Id          { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Description { get; init; } = "";
    public Color  Color       { get; init; } = Color.White;

    /// <summary>Salary paid every EconomySystem.SalaryInterval seconds.</summary>
    public int SalaryAmount { get; init; } = 100;

    /// <summary>Max players allowed in this job at once. -1 = unlimited.</summary>
    public int MaxSlots { get; init; } = -1;

    /// <summary>Whether this job can arrest other players.</summary>
    public bool CanArrest { get; init; } = false;

    /// <summary>Whether this job can set players as Wanted.</summary>
    public bool CanSetWanted { get; init; } = false;
}
