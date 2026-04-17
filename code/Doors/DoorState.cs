using System;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Attach to every door object in the map.
/// Tracks ownership, lock state, and purchase price.
///
/// NETWORKING:
///   Door objects must have NetworkMode set (host-owned) in the editor
///   so [Sync] properties replicate to all clients.
///   In the scene inspector: select the door → Network → Mode → Object.
///
/// OWNERSHIP:
///   OwnerSteamId stores the buyer's SteamId (ulong).
///   If Connection.SteamId is named differently in your s&box version,
///   change that one property — nowhere else stores it.
/// </summary>
public sealed class DoorState : Component
{
    // ── Editor configuration ──────────────────────────────────────────────

    /// <summary>Human-readable name shown in the door HUD.</summary>
    [Property] public string DoorLabel    { get; set; } = "Door";

    /// <summary>Purchase price set in the editor per door.</summary>
    [Property] public int    DefaultPrice { get; set; } = 1000;

    // ── Synced runtime state ──────────────────────────────────────────────

    [Sync] public ulong  OwnerSteamId { get; private set; }
    [Sync] public string OwnerName    { get; private set; } = "";
    [Sync] public int    BuyPrice     { get; private set; }
    [Sync] public bool   IsLocked     { get; private set; }

    // ── Derived helpers (read on any machine) ─────────────────────────────

    public bool IsOwned => OwnerSteamId != 0;

    public bool IsOwnedBy( Connection conn )
        => conn is not null && conn.SteamId == OwnerSteamId;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    protected override void OnStart()
    {
        if ( Networking.IsHost )
            BuyPrice = DefaultPrice;
    }

    // ── Host-only mutations ───────────────────────────────────────────────

    public void SetOwner( Connection conn )
    {
        AssertHost();
        OwnerSteamId = conn.SteamId;
        OwnerName    = conn.DisplayName;
        IsLocked     = false;
    }

    public void ClearOwner()
    {
        AssertHost();
        OwnerSteamId = 0;
        OwnerName    = "";
        IsLocked     = false;
    }

    public void SetLocked( bool locked )
    {
        AssertHost();
        IsLocked = locked;
        // TODO M8: animate the door model open/close here
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Sell-back value is half the original purchase price, floored to $1.
    /// </summary>
    public int SellValue => Math.Max( 1, BuyPrice / 2 );

    private static void AssertHost()
    {
        if ( !Networking.IsHost )
            throw new InvalidOperationException(
                "DoorState mutations must run on the host." );
    }
}
