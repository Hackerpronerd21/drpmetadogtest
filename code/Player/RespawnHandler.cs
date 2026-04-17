using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Manages player health and respawn.
/// Attach to the player pawn (NetworkMode 2, owner-driven).
///
/// NETWORKING:
///   Health is [Sync] — host writes, clients read.
///   TakeDamage is [Rpc.Host] — any source calls it on the host.
///   On death the host waits RespawnDelay seconds then teleports the
///   pawn to a spawn point and restores health.
///
/// MONEY PENALTY:
///   On death the player loses 10% of cash (min $0), announced in chat.
/// </summary>
public sealed class RespawnHandler : Component
{
    [Property] public float MaxHealth     { get; set; } = 100f;
    [Property] public float RespawnDelay  { get; set; } = 5f;

    [Sync] public float Health { get; private set; } = 100f;
    [Sync] public bool  IsDead { get; private set; }

    protected override void OnStart()
    {
        if ( !Networking.IsHost ) return;
        Health = MaxHealth;
        IsDead = false;
    }

    // ── Public API (host-only) ────────────────────────────────────────────

    /// <summary>
    /// Called by any host-side code to deal damage (e.g. fall, weapon, etc.).
    /// A future weapon system would call this directly.
    /// </summary>
    [Rpc.Host]
    public void RequestTakeDamage( float amount )
    {
        if ( !Networking.IsHost ) return;
        ApplyDamage( amount );
    }

    // ── Internal host logic ───────────────────────────────────────────────

    private void ApplyDamage( float amount )
    {
        if ( IsDead ) return;
        Health = System.Math.Max( 0f, Health - amount );

        if ( Health <= 0f )
            _ = Die();
    }

    private async Task Die()
    {
        IsDead = true;

        // Money penalty — 10 % of current wallet, floored at 0
        var state = Components.Get<PlayerState>();
        if ( state is not null )
        {
            int penalty = (int)( state.Money * 0.10f );
            if ( penalty > 0 )
            {
                state.SpendMoney( penalty );
                Announce( $"{OwnerName()} died and lost ${penalty}." );
            }
            else
            {
                Announce( $"{OwnerName()} died." );
            }
        }

        // Wait before respawning
        await GameTask.DelaySeconds( RespawnDelay );

        if ( !IsValid ) return;

        Respawn();
    }

    private void Respawn()
    {
        // Pick a random spawn point
        var spawns = Scene.GetAllComponents<SpawnPoint>().ToList();
        Vector3 spawnPos = spawns.Count > 0
            ? spawns[Game.Random.Int( 0, spawns.Count - 1 )].Transform.Position
            : Vector3.Up * 100f;

        GameObject.Transform.Position = spawnPos;
        Health = MaxHealth;
        IsDead = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private string OwnerName()
        => Network.OwnerConnection?.DisplayName ?? "Unknown";

    private void Announce( string text )
    {
        // Broadcast via any ChatSystem (all share the same static history)
        var chat = Scene.GetAllComponents<ChatSystem>().FirstOrDefault();
        chat?.BroadcastMessage( "Server", Color.Red, text );
    }
}
