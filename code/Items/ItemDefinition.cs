using Sandbox;

namespace DarkRp;

/// <summary>
/// Pure data — describes one item type. No logic lives here.
/// Add new items in ItemRegistry, not here.
/// </summary>
public sealed class ItemDefinition
{
    public string Id          { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Description { get; init; } = "";

    /// <summary>Logical category used by dealers to filter their stock.</summary>
    public string Category    { get; init; } = "misc"; // "weapon" | "medical" | "tool" | "misc"

    /// <summary>Base purchase price from a dealer.</summary>
    public int    Price       { get; init; } = 0;

    /// <summary>Max units in one inventory slot. 1 = no stacking.</summary>
    public int    MaxStack    { get; init; } = 1;

    /// <summary>UI accent color shown in the shop and inventory bar.</summary>
    public Color  Color       { get; init; } = Color.White;
}
