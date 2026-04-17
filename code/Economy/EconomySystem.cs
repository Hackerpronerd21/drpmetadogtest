using System.Linq;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Handles salary payouts on a fixed interval.
/// Runs on the host only — clients never tick this.
///
/// Setup: Add this component to a persistent host-owned GameObject in your scene
/// (e.g. a "GameManager" object). It will auto-find all PlayerState components.
/// </summary>
public sealed class EconomySystem : Component
{
    /// <summary>Seconds between salary payouts.</summary>
    [Property] public float SalaryInterval { get; set; } = 300f; // 5 minutes

    private float _timer;

    protected override void OnUpdate()
    {
        // Only the host drives economy
        if ( !Networking.IsHost ) return;

        _timer += Time.Delta;
        if ( _timer < SalaryInterval ) return;

        _timer = 0f;
        PaySalaries();
    }

    private void PaySalaries()
    {
        // Mayor's tax rate reduces net salary. Rate 0.0–0.5 (0%–50%).
        var   mayorSystem = Scene.GetAllComponents<MayorSystem>().FirstOrDefault();
        float taxRate     = mayorSystem?.TaxRate ?? 0f;

        foreach ( var state in Scene.GetAllComponents<PlayerState>() )
        {
            int gross = state.Job.SalaryAmount;
            if ( gross <= 0 ) continue;

            int tax = (int)( gross * taxRate );
            int net = System.Math.Max( 1, gross - tax ); // always pay at least $1
            state.GiveMoney( net );
        }
    }

    // ── Public API for one-off transactions ───────────────────────────────

    /// <summary>Transfer money between two players. Returns false if sender is broke.</summary>
    public static bool Transfer( PlayerState from, PlayerState to, int amount )
    {
        if ( !Networking.IsHost ) return false;
        if ( amount <= 0 ) return false;
        if ( !from.SpendMoney( amount ) ) return false;
        to.GiveMoney( amount );
        return true;
    }
}
