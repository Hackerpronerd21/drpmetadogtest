using Sandbox;

namespace DarkRp;

/// <summary>
/// Handles wanted flags and arresting players.
///
/// RULES enforced here (all server-side):
///   - Only players with CanArrest jobs can arrest.
///   - Only players with CanSetWanted jobs can mark others wanted.
///   - Police cannot arrest other police.
///   - Arrested players are teleported to jail (set JailPosition in scene).
///
/// Add this component to the same host GameManager object as EconomySystem.
/// </summary>
public sealed class ArrestSystem : Component
{
    /// <summary>Set this to your jail spawn point in the scene.</summary>
    [Property] public GameObject? JailSpawn { get; set; }

    /// <summary>How long (seconds) a player stays arrested before auto-release.</summary>
    [Property] public float JailDuration { get; set; } = 120f;

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Officer arrests target. Returns false if the action is not permitted.
    /// </summary>
    public bool TryArrest( PlayerState officer, PlayerState target )
    {
        if ( !Networking.IsHost ) return false;
        if ( !officer.Job.CanArrest ) return false;
        if ( target.IsArrested ) return false;

        // Police can't arrest other police
        if ( target.Job.CanArrest ) return false;

        target.SetArrested( true );
        SendToJail( target );
        ScheduleRelease( target );

        return true;
    }

    /// <summary>
    /// Mark a player as wanted. Only officers/mayor can do this.
    /// </summary>
    public bool TrySetWanted( PlayerState caller, PlayerState target, bool wanted )
    {
        if ( !Networking.IsHost ) return false;
        if ( !caller.Job.CanSetWanted ) return false;
        if ( target.IsArrested ) return false;

        target.SetWanted( wanted );
        return true;
    }

    /// <summary>Immediately release a player from jail.</summary>
    public void Release( PlayerState target )
    {
        if ( !Networking.IsHost ) return;
        target.SetArrested( false );
    }

    // ── Internals ─────────────────────────────────────────────────────────

    private void SendToJail( PlayerState target )
    {
        if ( JailSpawn is null ) return;

        // Move the player's pawn to the jail spawn position
        var pawn = target.GameObject;
        pawn.WorldPosition = JailSpawn.WorldPosition;
    }

    private async void ScheduleRelease( PlayerState target )
    {
        await Task.DelaySeconds( JailDuration );

        // Guard: player may have disconnected or been manually released
        if ( target is null || !target.IsValid() ) return;
        if ( !target.IsArrested ) return;

        Release( target );
    }
}
