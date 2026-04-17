using Sandbox;

namespace DarkRp;

/// <summary>
/// Pure data — describes one job. No game logic lives here.
/// Add new jobs in JobRegistry, not here.
/// </summary>
public sealed class JobDefinition
{
    public string   Id          { get; init; } = "";
    public string   DisplayName { get; init; } = "";
    public string   Description { get; init; } = "";
    public string   Department  { get; init; } = "Civilian";
    public Color    Color       { get; init; } = Color.White;
    public JobTier  Tier        { get; init; } = JobTier.Free;

    /// <summary>
    /// Real-world-inspired hourly wage reference in in-game dollars.
    /// Based on lower-end U.S. pay rates (yearly / 2080 for salaried roles).
    /// Crime jobs are 0 — they earn through crime payouts, not wages.
    /// </summary>
    public int HourlyWage { get; init; } = 0;

    /// <summary>
    /// Salary paid every EconomySystem.SalaryInterval seconds.
    /// Derived as HourlyWage * 3 so the EconomySystem can calibrate one interval
    /// to equal roughly 20 in-game minutes (3 intervals per game-hour).
    /// Crime/underground jobs use 0.
    /// </summary>
    public int SalaryAmount { get; init; } = 0;

    /// <summary>Max players allowed in this job at once. -1 = unlimited.</summary>
    public int MaxSlots { get; init; } = -1;

    /// <summary>Whether this job can arrest other players.</summary>
    public bool CanArrest { get; init; } = false;

    /// <summary>Whether this job can set players as Wanted.</summary>
    public bool CanSetWanted { get; init; } = false;
}
