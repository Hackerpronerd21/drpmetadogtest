using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Simple slot-based inventory. Synced as a JSON string to keep networking
/// straightforward — swap to a proper NetList when the item count grows.
///
/// NETWORKING: Server writes, [Sync] replicates to clients.
/// Clients read Slots for display; they never modify inventory directly.
/// </summary>
public sealed class DarkRpInventory : Component
{
    // Single JSON blob keeps sync simple. Parse on demand.
    [Sync] public string InventoryJson { get; private set; } = "[]";

    // ── Read API (safe to call on any machine) ────────────────────────────

    public IReadOnlyList<InventorySlot> Slots
        => JsonSerializer.Deserialize<List<InventorySlot>>( InventoryJson )
           ?? new List<InventorySlot>();

    public bool HasItem( string itemId, int count = 1 )
        => Slots.FirstOrDefault( s => s.ItemId == itemId )?.Count >= count;

    public int CountOf( string itemId )
        => Slots.FirstOrDefault( s => s.ItemId == itemId )?.Count ?? 0;

    // ── Write API (host only) ─────────────────────────────────────────────

    public void AddItem( string itemId, int count = 1 )
    {
        AssertHost();
        var slots = GetMutableSlots();
        var slot = slots.Find( s => s.ItemId == itemId );
        if ( slot is not null )
            slot.Count += count;
        else
            slots.Add( new InventorySlot { ItemId = itemId, Count = count } );
        Commit( slots );
    }

    /// <summary>Returns true if the item was present and removed.</summary>
    public bool RemoveItem( string itemId, int count = 1 )
    {
        AssertHost();
        var slots = GetMutableSlots();
        var slot = slots.Find( s => s.ItemId == itemId );
        if ( slot is null || slot.Count < count ) return false;
        slot.Count -= count;
        if ( slot.Count <= 0 ) slots.Remove( slot );
        Commit( slots );
        return true;
    }

    public void ClearInventory()
    {
        AssertHost();
        InventoryJson = "[]";
    }

    // ── Internals ─────────────────────────────────────────────────────────

    private List<InventorySlot> GetMutableSlots()
        => JsonSerializer.Deserialize<List<InventorySlot>>( InventoryJson )
           ?? new List<InventorySlot>();

    private void Commit( List<InventorySlot> slots )
        => InventoryJson = JsonSerializer.Serialize( slots );

    private static void AssertHost()
    {
        if ( !Networking.IsHost )
            throw new System.InvalidOperationException(
                "DarkRpInventory mutations must run on the host." );
    }
}

// ── Data ──────────────────────────────────────────────────────────────────

public sealed class InventorySlot
{
    public string ItemId { get; set; } = "";
    public int    Count  { get; set; } = 1;
}
