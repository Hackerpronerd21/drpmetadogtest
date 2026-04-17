using System.Linq;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Attach to the player pawn alongside PlayerState.
/// Handles proximity-based arrest and wanted-flag toggling.
///
/// FLOW:
///   Z (Arrest)     — arrests the nearest WANTED player within InteractRange.
///   X (MarkWanted) — toggles wanted on the nearest non-cop within InteractRange.
///
/// Both actions fire [Rpc.Host] calls so all validation remains server-side.
/// The client only decides WHO to target; the server decides whether the
/// action is permitted.
/// </summary>
public sealed class ArrestInteraction : Component
{
    [Property] public float InteractRange { get; set; } = 150f;

    protected override void OnUpdate()
    {
        // Only the local owner processes input — proxies skip entirely.
        if ( IsProxy ) return;

        var myState = Components.Get<PlayerState>();
        if ( myState is null ) return;

        if ( Input.Pressed( "Arrest" ) && myState.Job.CanArrest )
        {
            var target = FindNearestWanted();
            if ( target is not null )
                RequestArrest( target.GameObject );
        }

        if ( Input.Pressed( "MarkWanted" ) && myState.Job.CanSetWanted )
        {
            var target = FindNearestNonCop();
            if ( target is not null )
                RequestMarkWanted( target.GameObject, !target.IsWanted );
        }
    }

    // ── RPCs ─────────────────────────────────────────────────────────────
    // These run on the host. The host re-resolves PlayerState from the
    // passed GameObject so a cheating client cannot forge different targets.

    [Rpc.Host]
    public void RequestArrest( GameObject targetObject )
    {
        var myState     = Components.Get<PlayerState>();
        var targetState = targetObject?.Components.Get<PlayerState>();
        if ( myState is null || targetState is null ) return;

        var system = Scene.GetAllComponents<ArrestSystem>().FirstOrDefault();
        system?.TryArrest( myState, targetState );
    }

    [Rpc.Host]
    public void RequestMarkWanted( GameObject targetObject, bool wanted )
    {
        var myState     = Components.Get<PlayerState>();
        var targetState = targetObject?.Components.Get<PlayerState>();
        if ( myState is null || targetState is null ) return;

        var system = Scene.GetAllComponents<ArrestSystem>().FirstOrDefault();
        system?.TrySetWanted( myState, targetState, wanted );
    }

    // ── Proximity helpers ─────────────────────────────────────────────────
    // Plain foreach beats LINQ here — one pass, no allocation, early exit.

    private PlayerState? FindNearestWanted()
    {
        PlayerState? nearest    = null;
        float        bestDist   = InteractRange;

        foreach ( var state in Scene.GetAllComponents<PlayerState>() )
        {
            if ( state.GameObject == GameObject ) continue;
            if ( !state.IsWanted ) continue;

            float dist = Vector3.DistanceBetween( WorldPosition, state.WorldPosition );
            if ( dist < bestDist )
            {
                nearest  = state;
                bestDist = dist;
            }
        }

        return nearest;
    }

    private PlayerState? FindNearestNonCop()
    {
        PlayerState? nearest    = null;
        float        bestDist   = InteractRange;

        foreach ( var state in Scene.GetAllComponents<PlayerState>() )
        {
            if ( state.GameObject == GameObject ) continue;
            if ( state.Job.CanArrest ) continue; // skip fellow officers

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
