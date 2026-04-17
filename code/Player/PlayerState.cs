using System;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Core networked player state: money, job, wanted, arrested.
///
/// NETWORKING MODEL:
///   - Attach this component to the player pawn GameObject.
///   - [Sync] properties replicate from the network owner to all clients.
///   - All mutation methods guard with Networking.IsHost — the server is
///     the only caller. Clients request changes via [Rpc.Host] methods.
///
/// ANTI-EXPLOIT:
///   - Clients never set money/job/arrest directly.
///   - Every client-initiated action goes through an [Rpc.Host] method
///     where the server validates before applying.
/// </summary>
public sealed class PlayerState : Component
{
    // ── Synced state ──────────────────────────────────────────────────────

    [Sync] public int    Money      { get; private set; } = 500;
    [Sync] public string JobId      { get; private set; } = "citizen";
    [Sync] public bool   IsWanted   { get; private set; }
    [Sync] public bool   IsArrested { get; private set; }

    // Resolved from registry — no extra network traffic needed.
    public JobDefinition Job => JobRegistry.Get( JobId );

    // ── Server-only mutations (call from game systems, not from clients) ──

    public void GiveMoney( int amount )
    {
        AssertHost();
        if ( amount <= 0 ) return;
        Money += amount;
    }

    public void TakeMoney( int amount )
    {
        AssertHost();
        if ( amount <= 0 ) return;
        Money = Math.Max( 0, Money - amount );
    }

    /// <summary>Returns false if the player cannot afford it.</summary>
    public bool SpendMoney( int amount )
    {
        AssertHost();
        if ( Money < amount ) return false;
        Money -= amount;
        return true;
    }

    public void AssignJob( string jobId )
    {
        AssertHost();
        if ( !JobRegistry.Exists( jobId ) ) return;
        JobId = jobId;
    }

    public void SetWanted( bool wanted )
    {
        AssertHost();
        IsWanted = wanted;
    }

    public void SetArrested( bool arrested )
    {
        AssertHost();
        IsArrested = arrested;
        if ( arrested ) IsWanted = false; // arrest clears wanted flag
    }

    // ── Client → Server RPCs ──────────────────────────────────────────────

    /// <summary>
    /// Client requests a job change. Server checks slot availability and
    /// other conditions before applying.
    /// </summary>
    [Rpc.Host]
    public void RequestJobChange( string jobId )
    {
        if ( IsArrested ) return;
        if ( !JobRegistry.Exists( jobId ) ) return;
        if ( !JobRegistry.IsJobAvailable( jobId ) ) return;
        // TODO: add cooldown, prerequisite item checks here
        AssignJob( jobId );
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void AssertHost()
    {
        if ( !Networking.IsHost )
            throw new System.InvalidOperationException(
                "PlayerState mutations must run on the host." );
    }
}
