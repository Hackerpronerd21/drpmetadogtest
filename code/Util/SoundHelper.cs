using Sandbox;

namespace DarkRp;

/// <summary>
/// Centralised stubs for all DarkRP sounds.
/// Replace the string constants with real asset paths once you have audio files.
///
/// Usage:
///   SoundHelper.PlayAt( SoundHelper.Paths.Arrest, position );
///   SoundHelper.PlayUi( SoundHelper.Paths.Purchase );
/// </summary>
public static class SoundHelper
{
    public static class Paths
    {
        // Player / world
        public const string Arrest    = "sounds/darkrp/arrest.sound";
        public const string Release   = "sounds/darkrp/release.sound";
        public const string Death     = "sounds/darkrp/death.sound";
        public const string Respawn   = "sounds/darkrp/respawn.sound";

        // Economy
        public const string Purchase  = "sounds/darkrp/purchase.sound";
        public const string Salary    = "sounds/darkrp/salary.sound";
        public const string MoneyLoss = "sounds/darkrp/moneyloss.sound";

        // Doors
        public const string DoorBuy   = "sounds/darkrp/door_buy.sound";
        public const string DoorSell  = "sounds/darkrp/door_sell.sound";
        public const string DoorLock  = "sounds/darkrp/door_lock.sound";
        public const string DoorUnlock= "sounds/darkrp/door_unlock.sound";

        // Mayor / laws
        public const string LawProclaim = "sounds/darkrp/law_proclaim.sound";
        public const string LawRepeal   = "sounds/darkrp/law_repeal.sound";
    }

    /// <summary>Play a 3-D positioned sound. No-ops if path is missing.</summary>
    public static void PlayAt( string path, Vector3 position )
    {
        Sound.Play( path, position );
    }

    /// <summary>Play a 2-D UI sound on the local client. No-ops if path is missing.</summary>
    public static void PlayUi( string path )
    {
        Sound.Play( path );
    }
}
