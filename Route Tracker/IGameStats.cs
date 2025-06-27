using System;
using System.Collections.Generic;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Interface defining the contract for game statistics access
    // Provides methods for updating, retrieving, and monitoring game statistics
    // Enables consistent interaction with different game implementations
    // ==========MY NOTES==============
    // This defines what any game stats class must implement
    // Makes it possible to support different games with the same UI
    public interface IGameStats
    {
        event EventHandler<StatsUpdatedEventArgs>? StatsUpdated;
        void StartUpdating();
        void StopUpdating();
        Dictionary<string, object> GetStatsAsDictionary();

        (int Percent, float PercentFloat, int Viewpoints, int Myan, int Treasure,
         int Fragments, int Assassin, int Naval, int Letters, int Manuscripts,
         int Music, int Forts, int Taverns, int TotalChests) GetStats();

        (int StoryMissions, int TemplarHunts, int LegendaryShips, int TreasureMaps) GetSpecialActivityCounts();
    }
}