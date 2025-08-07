using System;
using System.Collections.Generic;

namespace Route_Tracker
{
    // This defines what any game stats class must implement
    // Makes it possible to support different games with the same UI
    // Uses dictionaries for flexibility so each game can have its own stats
    public interface IGameStats
    {
        // Event raised when stats are updated
        event EventHandler<GameStatsEventArgs>? StatsUpdated;

        // Start and stop automatic updates
        void StartUpdating();
        void StopUpdating();

        // Primary method for getting all game statistics in a flexible format
        Dictionary<string, object> GetStatsAsDictionary();
    }
}