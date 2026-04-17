using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Attach to the GameManager object (NetworkMode: Object / host-owned).
/// Holds the server's active law list and tax rate.
///
/// NETWORKING:
///   GameManager must have NetworkMode = Object in the editor so that
///   [Sync] replicates to clients and [Rpc.Host] methods are callable remotely.
///
/// LAWS:
///   Stored as a JSON string for easy [Sync]. Max 10 laws, 128 chars each.
///   LawsHud.razor reads Laws property every frame — no extra event needed.
///
/// TAX:
///   TaxRate (0.0–0.5) is read by EconomySystem.PaySalaries each tick.
///   At 0.20 a $150 salary becomes $120 net.
/// </summary>
public sealed class MayorSystem : Component
{
    private const int MaxLaws      = 10;
    private const int MaxLawLength = 128;

    // ── Synced state ──────────────────────────────────────────────────────

    [Sync] public string LawsJson { get; private set; } = "[]";
    [Sync] public float  TaxRate  { get; private set; } = 0f;

    // ── Read API (any machine) ────────────────────────────────────────────

    public IReadOnlyList<string> Laws
        => JsonSerializer.Deserialize<List<string>>( LawsJson )
           ?? new List<string>();

    // ── Client → Host RPCs ────────────────────────────────────────────────

    [Rpc.Host]
    public void RequestAddLaw( string law )
    {
        var caller = FindCallerState();
        if ( caller?.JobId != "mayor" ) return;

        law = law.Trim();
        if ( string.IsNullOrEmpty( law ) || law.Length > MaxLawLength ) return;

        var list = MutableLaws();
        if ( list.Count >= MaxLaws ) return;
        if ( list.Contains( law, StringComparer.OrdinalIgnoreCase ) ) return;

        list.Add( law );
        CommitLaws( list );
        Announce( $"New law: \"{law}\"" );
    }

    [Rpc.Host]
    public void RequestRemoveLaw( int index )
    {
        var caller = FindCallerState();
        if ( caller?.JobId != "mayor" ) return;

        var list = MutableLaws();
        if ( index < 0 || index >= list.Count ) return;

        string removed = list[index];
        list.RemoveAt( index );
        CommitLaws( list );
        Announce( $"Law repealed: \"{removed}\"" );
    }

    [Rpc.Host]
    public void RequestSetTaxRate( float rate )
    {
        var caller = FindCallerState();
        if ( caller?.JobId != "mayor" ) return;

        rate = Math.Clamp( rate, 0f, 0.5f );
        int oldPct = (int)( TaxRate * 100 );
        int newPct = (int)( rate    * 100 );
        TaxRate    = rate;

        Announce( $"Tax rate changed from {oldPct}% to {newPct}%." );
    }

    [Rpc.Host]
    public void RequestClearLaws()
    {
        var caller = FindCallerState();
        if ( caller?.JobId != "mayor" ) return;

        LawsJson = "[]";
        Announce( "All laws have been repealed." );
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private PlayerState? FindCallerState()
        => Scene.GetAllComponents<PlayerState>()
            .FirstOrDefault( s => s.Network.OwnerConnection == Rpc.Caller );

    private List<string> MutableLaws()
        => JsonSerializer.Deserialize<List<string>>( LawsJson )
           ?? new List<string>();

    private void CommitLaws( List<string> list )
        => LawsJson = JsonSerializer.Serialize( list );

    private void Announce( string message )
    {
        // Route through any available ChatSystem — [Rpc.Broadcast] sends to all.
        var chat = Scene.GetAllComponents<ChatSystem>().FirstOrDefault();
        chat?.BroadcastMessage( "Mayor", JobRegistry.Get( "mayor" ).Color, message );
    }
}
