using Sandbox;

namespace DarkRp;

/// <summary>
/// Immutable chat message. Stored in ChatSystem.History (client-side only).
/// </summary>
public sealed record ChatMessage( string SenderName, Color JobColor, string Text );
