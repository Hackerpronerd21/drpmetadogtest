using System;
using System.Linq;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Attach to the player pawn alongside PlayerState.
/// Handles two responsibilities:
///
///   1. CLIENT — detects nearby sellers on Use (E) and fires ShopOpenRequested
///              so ShopMenu.razor can open for the right dealer.
///
///   2. HOST   — validates and executes buy requests from clients.
///
/// ANTI-EXPLOIT:
///   - Server re-checks proximity (buyer must be within range of dealer).
///   - Server re-checks dealer job eligibility.
///   - Money transfer uses EconomySystem.Transfer which guards with AssertHost.
///   - Item ID is validated against ItemRegistry before any state changes.
/// </summary>
public sealed class ShopInteraction : Component
{
    [Property] public float ShopRange { get; set; } = 150f;

    // ── Static event — ShopMenu subscribes to this ────────────────────────
    // Fired on the local client when the player opens a shop.

    public static event Action<PlayerState>? ShopOpenRequested;

    // ── Client input ──────────────────────────────────────────────────────

    protected override void OnUpdate()
    {
        if ( IsProxy ) return;

        var myState = Components.Get<PlayerState>();
        if ( myState is null ) return;

        // Sellers don't shop at each other
        if ( ItemRegistry.IsSeller( myState.JobId ) ) return;

        if ( Input.Pressed( "Use" ) )
        {
            var seller = FindNearbySeller();
            if ( seller is not null )
                ShopOpenRequested?.Invoke( seller );
        }
    }

    // ── Client → Host RPC ─────────────────────────────────────────────────

    /// <summary>
    /// Client requests to buy one unit of an item from a specific dealer.
    /// All validation runs on the host.
    /// </summary>
    [Rpc.Host]
    public void RequestBuy( GameObject dealerObject, string itemId, int quantity )
    {
        if ( quantity <= 0 || quantity > 99 ) return;
        if ( !ItemRegistry.Exists( itemId ) ) return;

        var buyer     = Components.Get<PlayerState>();
        var buyerInv  = Components.Get<DarkRpInventory>();
        var dealer    = dealerObject?.Components.Get<PlayerState>();

        if ( buyer is null || buyerInv is null || dealer is null ) return;

        // Dealer must still hold a selling job
        if ( !ItemRegistry.IsSeller( dealer.JobId ) ) return;

        // Item must be in the dealer's current stock
        var stock = ItemRegistry.GetDealerStock( dealer.JobId );
        var found = false;
        foreach ( var i in stock )
            if ( i.Id == itemId ) { found = true; break; }
        if ( !found ) return;

        // Server-side proximity check — client cannot fake their position
        float dist = Vector3.DistanceBetween( GameObject.WorldPosition, dealerObject.WorldPosition );
        if ( dist > ShopRange * 1.5f ) return; // 1.5× tolerance for network lag

        var item      = ItemRegistry.Get( itemId );
        int totalCost = item.Price * quantity;

        // Transfer pays the dealer and deducts from the buyer atomically
        if ( !EconomySystem.Transfer( buyer, dealer, totalCost ) ) return;

        buyerInv.AddItem( itemId, quantity );

        // Broadcast a chat notification so everyone sees the transaction
        var chat = Scene.GetAllComponents<ChatSystem>()
            .FirstOrDefault( c => c.Network.OwnerConnection == buyer.Network.OwnerConnection );

        string buyerName  = buyer.Network.OwnerConnection?.DisplayName  ?? "?";
        string dealerName = dealer.Network.OwnerConnection?.DisplayName ?? "?";
        chat?.BroadcastMessage( "Server", Color.Yellow,
            $"{buyerName} bought {item.DisplayName} ×{quantity} from {dealerName} for ${totalCost}." );
    }

    // ── Proximity helper ──────────────────────────────────────────────────

    private PlayerState? FindNearbySeller()
    {
        PlayerState? nearest  = null;
        float        bestDist = ShopRange;

        foreach ( var state in Scene.GetAllComponents<PlayerState>() )
        {
            if ( state.GameObject == GameObject ) continue;
            if ( !ItemRegistry.IsSeller( state.JobId ) ) continue;

            float dist = Vector3.DistanceBetween( WorldPosition, state.WorldPosition );
            if ( dist < bestDist )
            {
                nearest  = state;
                bestDist = dist;
            }
        }

        return nearest;
    }
}
