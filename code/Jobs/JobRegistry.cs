using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace DarkRp;

/// <summary>
/// Single source of truth for all job definitions.
/// Server and client both read this — it is never modified at runtime.
/// To add a job: append to the _jobs list below. That's it.
/// </summary>
public static class JobRegistry
{
    private static readonly Dictionary<string, JobDefinition> _jobs;

    static JobRegistry()
    {
        var list = new List<JobDefinition>
        {
            new()
            {
                Id          = "citizen",
                DisplayName = "Citizen",
                Description = "An ordinary resident of the city.",
                Color       = Color.White,
                SalaryAmount = 50,
                MaxSlots    = -1
            },
            new()
            {
                Id           = "police",
                DisplayName  = "Police Officer",
                Description  = "Enforces the law. Can arrest wanted players.",
                Color        = new Color( 0.3f, 0.5f, 1f ),
                SalaryAmount = 150,
                MaxSlots     = 4,
                CanArrest    = true,
                CanSetWanted = true
            },
            new()
            {
                Id           = "medic",
                DisplayName  = "Medic",
                Description  = "Heals players for a fee.",
                Color        = new Color( 0.3f, 1f, 0.3f ),
                SalaryAmount = 120,
                MaxSlots     = 2
            },
            new()
            {
                Id           = "dealer",
                DisplayName  = "Gun Dealer",
                Description  = "Sells weapons to other players.",
                Color        = new Color( 1f, 0.7f, 0.1f ),
                SalaryAmount = 100,
                MaxSlots     = 2
            },
            new()
            {
                Id           = "mayor",
                DisplayName  = "Mayor",
                Description  = "Sets tax rate and city laws.",
                Color        = new Color( 1f, 0.85f, 0f ),
                SalaryAmount = 200,
                MaxSlots     = 1,
                CanSetWanted = true
            },
        };

        _jobs = list.ToDictionary( j => j.Id );
    }

    public static JobDefinition Get( string id )
        => _jobs.TryGetValue( id, out var job ) ? job : _jobs["citizen"];

    public static bool Exists( string id )
        => _jobs.ContainsKey( id );

    public static IReadOnlyCollection<JobDefinition> All
        => _jobs.Values;

    /// <summary>Returns how many players currently hold this job.</summary>
    public static int CountPlayersInJob( string jobId )
        => Game.ActiveScene?
            .GetAllComponents<PlayerState>()
            .Count( s => s.JobId == jobId ) ?? 0;

    /// <summary>Returns false if the job is full.</summary>
    public static bool IsJobAvailable( string jobId )
    {
        var def = Get( jobId );
        if ( def.MaxSlots < 0 ) return true;
        return CountPlayersInJob( jobId ) < def.MaxSlots;
    }
}
