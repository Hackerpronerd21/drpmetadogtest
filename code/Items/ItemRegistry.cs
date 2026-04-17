using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Single source of truth for all item definitions.
/// Read-only at runtime — never modified after startup.
///
/// DEALER STOCK:
///   Each selling job has a category whitelist defined in GetDealerStock().
///   "dealer" sells weapons; "medic" sells medical items.
///   To add items: append to the list in the static constructor.
/// </summary>
public static class ItemRegistry
{
    private static readonly Dictionary<string, ItemDefinition> _items;

    static ItemRegistry()
    {
        var list = new List<ItemDefinition>
        {
            new()
            {
                Id          = "pistol",
                DisplayName = "Pistol",
                Description = "A standard sidearm. Reliable at short range.",
                Category    = "weapon",
                Price       = 200,
                MaxStack    = 1,
                Color       = new Color( 0.85f, 0.55f, 0.2f )
            },
            new()
            {
                Id          = "rifle",
                DisplayName = "Rifle",
                Description = "An assault rifle. High damage, harder to obtain.",
                Category    = "weapon",
                Price       = 500,
                MaxStack    = 1,
                Color       = new Color( 1f, 0.3f, 0.3f )
            },
            new()
            {
                Id          = "shotgun",
                DisplayName = "Shotgun",
                Description = "Devastating at close range.",
                Category    = "weapon",
                Price       = 350,
                MaxStack    = 1,
                Color       = new Color( 0.9f, 0.6f, 0.1f )
            },
            new()
            {
                Id          = "medkit",
                DisplayName = "Medkit",
                Description = "Restores health. Sold by the medic.",
                Category    = "medical",
                Price       = 150,
                MaxStack    = 5,
                Color       = new Color( 0.3f, 1f, 0.45f )
            },
            new()
            {
                Id          = "bandage",
                DisplayName = "Bandage",
                Description = "Cheap first aid. Stops bleeding.",
                Category    = "medical",
                Price       = 50,
                MaxStack    = 10,
                Color       = new Color( 0.9f, 0.9f, 0.9f )
            },
            new()
            {
                Id          = "lockpick",
                DisplayName = "Lockpick",
                Description = "Opens locked doors. Used by thieves.",
                Category    = "tool",
                Price       = 75,
                MaxStack    = 10,
                Color       = new Color( 0.7f, 0.7f, 0.3f )
            },
        };

        _items = list.ToDictionary( i => i.Id );
    }

    public static ItemDefinition  Get( string id ) =>
        _items.TryGetValue( id, out var item ) ? item : _items["pistol"];

    public static bool Exists( string id ) => _items.ContainsKey( id );

    public static IReadOnlyCollection<ItemDefinition> All => _items.Values;

    /// <summary>
    /// Returns the items a given job is allowed to sell.
    /// Extend this switch when new dealer-type jobs are added.
    /// </summary>
    public static IEnumerable<ItemDefinition> GetDealerStock( string jobId ) => jobId switch
    {
        "dealer" => _items.Values.Where( i => i.Category == "weapon" ),
        "medic"  => _items.Values.Where( i => i.Category == "medical" ),
        _        => Enumerable.Empty<ItemDefinition>()
    };

    /// <summary>Returns true if this job can sell anything.</summary>
    public static bool IsSeller( string jobId ) =>
        jobId is "dealer" or "medic";
}
