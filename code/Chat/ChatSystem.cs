using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Attach to the player pawn. Routes chat messages and /commands.
///
/// NETWORKING MODEL:
///   Client → SendMessage [Rpc.Host] → host validates/parses
///   Host   → BroadcastMessage [Rpc.Broadcast] → runs on ALL clients
///   Each client appends the message to the static History list.
///
/// History is static so a single shared list exists per client process,
/// regardless of which pawn's ChatSystem triggered the broadcast.
///
/// PLAYER COMMANDS:
///   /pay [name] [amount]       — transfer money to another player
///
/// ADMIN COMMANDS (host only):
///   /givemoney [name] [amount] — give money to a player
///   /setjob [name] [jobId]     — force-change a player's job
///   /kick [name]               — disconnect a player
/// </summary>
public sealed class ChatSystem : Component
{
    private const int MaxHistory = 20;

    // ── Client-side shared history ────────────────────────────────────────
    // Static: all ChatSystem instances on a client write to the same list.
    // ChatBox.razor reads this directly — no extra sync needed.

    public static readonly List<ChatMessage> History = new();

    /// <summary>Fired on every client when a new message arrives.</summary>
    public static event Action? OnMessageReceived;

    // ── Client → Host ─────────────────────────────────────────────────────

    /// <summary>
    /// Client calls this to send a message or command.
    /// Runs on the host — never trust the caller's own validation.
    /// </summary>
    [Rpc.Host]
    public void SendMessage( string raw )
    {
        if ( string.IsNullOrWhiteSpace( raw ) ) return;
        raw = raw.Trim();
        if ( raw.Length > 256 ) raw = raw[..256]; // cap length

        var sender = Components.Get<PlayerState>();
        if ( sender is null ) return;

        if ( raw.StartsWith( "/" ) )
            ProcessCommand( sender, raw );
        else
            BroadcastMessage( sender.Job.DisplayName, sender.Job.Color, raw );
    }

    // ── Command parsing (host only) ───────────────────────────────────────

    private void ProcessCommand( PlayerState sender, string raw )
    {
        var parts = raw.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
        if ( parts.Length == 0 ) return;

        switch ( parts[0].ToLowerInvariant() )
        {
            case "/pay":        HandlePay( sender, parts );        break;
            case "/givemoney":  HandleGiveMoney( sender, parts );  break;
            case "/setjob":     HandleSetJob( sender, parts );     break;
            case "/kick":       HandleKick( sender, parts );       break;

            default:
                BroadcastMessage( "Server", Color.Gray,
                    $"Unknown command: {parts[0]}" );
                break;
        }
    }

    // ── Helper: resolve player by partial name ────────────────────────────

    private PlayerState? FindPlayerByName( string nameQuery, PlayerState? exclude = null )
        => Scene.GetAllComponents<PlayerState>()
                .FirstOrDefault( s =>
                    ( exclude is null || s.GameObject != exclude.GameObject ) &&
                    ( s.Network.OwnerConnection?.DisplayName
                         .Contains( nameQuery, StringComparison.OrdinalIgnoreCase ) ?? false ) );

    private static bool IsAdminCaller()
        => Rpc.Caller.IsHost;

    // ── Player commands ───────────────────────────────────────────────────

    private void HandlePay( PlayerState sender, string[] parts )
    {
        // /pay <name> <amount>
        if ( parts.Length < 3 )
        {
            BroadcastMessage( "Server", Color.Gray, "Usage: /pay <name> <amount>" );
            return;
        }

        if ( !int.TryParse( parts[^1], out int amount ) || amount <= 0 )
        {
            BroadcastMessage( "Server", Color.Gray, "Amount must be a positive whole number." );
            return;
        }

        string nameQuery = string.Join( " ", parts[1..^1] );
        var target = FindPlayerByName( nameQuery, exclude: sender );

        if ( target is null )
        {
            BroadcastMessage( "Server", Color.Gray, $"Player \"{nameQuery}\" not found." );
            return;
        }

        if ( !EconomySystem.Transfer( sender, target, amount ) )
        {
            BroadcastMessage( "Server", Color.Gray,
                $"Insufficient funds. You have ${sender.Money}." );
            return;
        }

        string senderName = sender.Network.OwnerConnection?.DisplayName ?? "?";
        string targetName = target.Network.OwnerConnection?.DisplayName ?? "?";
        BroadcastMessage( "Server", Color.Yellow,
            $"{senderName} paid {targetName} ${amount}." );
    }

    // ── Admin commands ────────────────────────────────────────────────────

    private void HandleGiveMoney( PlayerState sender, string[] parts )
    {
        // /givemoney <name> <amount>
        if ( !IsAdminCaller() )
        {
            BroadcastMessage( "Server", Color.Gray, "You don't have permission to use that command." );
            return;
        }

        if ( parts.Length < 3 )
        {
            BroadcastMessage( "Server", Color.Gray, "Usage: /givemoney <name> <amount>" );
            return;
        }

        if ( !int.TryParse( parts[^1], out int amount ) || amount <= 0 )
        {
            BroadcastMessage( "Server", Color.Gray, "Amount must be a positive whole number." );
            return;
        }

        string nameQuery = string.Join( " ", parts[1..^1] );
        var target = FindPlayerByName( nameQuery );

        if ( target is null )
        {
            BroadcastMessage( "Server", Color.Gray, $"Player \"{nameQuery}\" not found." );
            return;
        }

        target.GiveMoney( amount );

        string targetName = target.Network.OwnerConnection?.DisplayName ?? "?";
        BroadcastMessage( "Server", Color.Yellow,
            $"Admin gave {targetName} ${amount}." );
    }

    private void HandleSetJob( PlayerState sender, string[] parts )
    {
        // /setjob <name> <jobId>
        if ( !IsAdminCaller() )
        {
            BroadcastMessage( "Server", Color.Gray, "You don't have permission to use that command." );
            return;
        }

        if ( parts.Length < 3 )
        {
            BroadcastMessage( "Server", Color.Gray, "Usage: /setjob <name> <jobId>" );
            return;
        }

        string jobId     = parts[^1].ToLowerInvariant();
        string nameQuery = string.Join( " ", parts[1..^1] );

        var job = JobRegistry.Get( jobId );
        if ( job is null )
        {
            BroadcastMessage( "Server", Color.Gray, $"Unknown job: \"{jobId}\"." );
            return;
        }

        var target = FindPlayerByName( nameQuery );
        if ( target is null )
        {
            BroadcastMessage( "Server", Color.Gray, $"Player \"{nameQuery}\" not found." );
            return;
        }

        target.AssignJob( jobId ); // host-direct call — no RPC needed since we're already on host

        string targetName = target.Network.OwnerConnection?.DisplayName ?? "?";
        BroadcastMessage( "Server", Color.Yellow,
            $"Admin set {targetName}'s job to {job.DisplayName}." );
    }

    private void HandleKick( PlayerState sender, string[] parts )
    {
        // /kick <name>
        if ( !IsAdminCaller() )
        {
            BroadcastMessage( "Server", Color.Gray, "You don't have permission to use that command." );
            return;
        }

        if ( parts.Length < 2 )
        {
            BroadcastMessage( "Server", Color.Gray, "Usage: /kick <name>" );
            return;
        }

        string nameQuery = string.Join( " ", parts[1..] );
        var target = FindPlayerByName( nameQuery );

        if ( target is null )
        {
            BroadcastMessage( "Server", Color.Gray, $"Player \"{nameQuery}\" not found." );
            return;
        }

        var conn = target.Network.OwnerConnection;
        if ( conn is null ) return;

        string targetName = conn.DisplayName;
        BroadcastMessage( "Server", Color.Orange, $"{targetName} was kicked by an admin." );

        target.GameObject.Destroy(); // kick: destroy their pawn; host will handle clean-up
    }

    // ── Host → All clients ────────────────────────────────────────────────

    /// <summary>
    /// Runs on every client. Appends to the shared static History.
    /// Host calls this after validating; clients never call it directly.
    /// </summary>
    [Rpc.Broadcast]
    public void BroadcastMessage( string senderName, Color jobColor, string text )
    {
        var msg = new ChatMessage( senderName, jobColor, text );

        History.Add( msg );
        if ( History.Count > MaxHistory )
            History.RemoveAt( 0 );

        OnMessageReceived?.Invoke();
    }
}
