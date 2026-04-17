using System;
using System.Linq;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Attach to the player pawn.
/// Detects the door the player is looking at (proximity + facing direction)
/// and fires events that DoorPanel.razor subscribes to.
///
/// KEYS:
///   E (Use) — opens door menu when looking at a door; closes menu if open.
///
/// FLOW:
///   Client side: updates CurrentDoor each frame, fires InteractRequested.
///   Host side  : RequestBuy / RequestSell / RequestToggleLock validate and apply.
///
/// ANTI-EXPLOIT:
///   All mutations validate Rpc.Caller's ownership before acting.
///   Buy checks door is unowned + buyer has the money.
///   Sell/lock checks Rpc.Caller.SteamId == door.OwnerSteamId.
/// </summary>
public sealed class DoorInteraction : Component
{
    [Property] public float LookRange { get; set; } = 200f;

    // ── Static state — DoorPanel.razor reads these every frame ────────────

    /// <summary>The door the local player is currently facing. Null if none.</summary>
    public static DoorState? CurrentDoor { get; private set; }

    /// <summary>
    /// Set to true by DoorPanel when its menu is open.
    /// Prevents DoorInteraction from re-firing InteractRequested
    /// on the same frame the panel closes.
    /// </summary>
    public static bool IsMenuOpen { get; set; }

    // ── Events ────────────────────────────────────────────────────────────

    /// <summary>Fired on the local client when Use is pressed on a door.</summary>
    public static event Action<DoorState>? InteractRequested;

    // ── Client update ─────────────────────────────────────────────────────

    protected override void OnUpdate()
    {
        if ( IsProxy ) return;

        CurrentDoor = FindLookedAtDoor();

        if ( CurrentDoor is not null && !IsMenuOpen && Input.Pressed( "Use" ) )
            InteractRequested?.Invoke( CurrentDoor );
    }

    // ── RPCs ──────────────────────────────────────────────────────────────

    [Rpc.Host]
    public void RequestBuyDoor( GameObject doorObject )
    {
        var door  = doorObject?.Components.Get<DoorState>();
        var buyer = Components.Get<PlayerState>();
        if ( door is null || buyer is null ) return;
        if ( door.IsOwned ) return;

        if ( !buyer.SpendMoney( door.BuyPrice ) ) return;

        door.SetOwner( Rpc.Caller );
        Announce( buyer, $"bought {door.DoorLabel} for ${door.BuyPrice}." );
    }

    [Rpc.Host]
    public void RequestSellDoor( GameObject doorObject )
    {
        var door   = doorObject?.Components.Get<DoorState>();
        var seller = Components.Get<PlayerState>();
        if ( door is null || seller is null ) return;
        if ( !door.IsOwnedBy( Rpc.Caller ) ) return;

        int refund = door.SellValue;
        seller.GiveMoney( refund );
        door.ClearOwner();
        Announce( seller, $"sold their property and received ${refund}." );
    }

    [Rpc.Host]
    public void RequestToggleLock( GameObject doorObject )
    {
        var door = doorObject?.Components.Get<DoorState>();
        if ( door is null ) return;
        if ( !door.IsOwnedBy( Rpc.Caller ) ) return;

        door.SetLocked( !door.IsLocked );
    }

    // ── Door detection ────────────────────────────────────────────────────
    // Uses proximity + dot product rather than a raycast.
    // The player must be within LookRange AND the door must be
    // within a ~45° cone in front of them.

    private DoorState? FindLookedAtDoor()
    {
        var forward = WorldRotation.Forward;

        DoorState? best    = null;
        float      bestDot = 0.7f; // cos(45°) ≈ 0.707

        foreach ( var door in Scene.GetAllComponents<DoorState>() )
        {
            float dist = Vector3.DistanceBetween( WorldPosition, door.WorldPosition );
            if ( dist > LookRange ) continue;

            var toTarget = ( door.WorldPosition - WorldPosition ).Normal;
            float dot    = Vector3.Dot( forward, toTarget );
            if ( dot > bestDot )
            {
                bestDot = dot;
                best    = door;
            }
        }

        return best;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void Announce( PlayerState actor, string message )
    {
        var chat = Scene.GetAllComponents<ChatSystem>()
            .FirstOrDefault( c => c.Network.OwnerConnection
                == actor.Network.OwnerConnection );

        string name = actor.Network.OwnerConnection?.DisplayName ?? "?";
        chat?.BroadcastMessage( "Server", Color.Yellow, $"{name} {message}" );
    }
}
