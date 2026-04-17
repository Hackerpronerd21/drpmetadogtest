using System.Collections.Generic;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Attach to a host-owned GameManager object in your scene.
/// Handles player spawning when a connection joins.
/// </summary>
public sealed class DarkRpGameManager : Component, Component.INetworkListener
{
    /// <summary>Drag the DarkRpPlayer.prefab here in the inspector.</summary>
    [Property] public GameObject? PlayerPrefab { get; set; }

    /// <summary>
    /// Spawn points — add empty GameObjects tagged "spawnpoint" in your scene,
    /// or just leave the list empty and players will spawn at the world origin.
    /// </summary>
    [Property] public List<GameObject> SpawnPoints { get; set; } = new();

    // INetworkListener: called on host when a client fully connects
    public void OnActive( Connection channel )
    {
        if ( PlayerPrefab is null )
        {
            Log.Error( "DarkRpGameManager: PlayerPrefab is not set!" );
            return;
        }

        var spawnPos = GetSpawnPosition();

        var playerGo = PlayerPrefab.Clone( spawnPos, Rotation.Identity );
        playerGo.Name = $"Player ({channel.DisplayName})";
        playerGo.Network.AssignOwnership( channel );
        playerGo.NetworkSpawn( channel );

        Log.Info( $"[DarkRP] Spawned player for {channel.DisplayName} at {spawnPos}" );
    }

    private Vector3 GetSpawnPosition()
    {
        if ( SpawnPoints.Count == 0 )
            return Vector3.Zero;

        // Pick a random spawn point
        var index = Game.Random.Int( 0, SpawnPoints.Count - 1 );
        return SpawnPoints[index].WorldPosition;
    }
}
