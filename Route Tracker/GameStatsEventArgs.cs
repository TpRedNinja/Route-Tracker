using System;
using System.Collections.Generic;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Event args class that carries game statistics data to event subscribers
    // Uses a flexible dictionary approach to support different stats for different games
    // ==========MY NOTES==============
    // This package contains all the stats in one neat bundle using a dictionary
    // When stats update, this gets sent to anyone listening for changes
    // Makes it easy to update the UI with all new values at once
    public class GameStatsEventArgs : EventArgs
    {
        // All game stats in a flexible dictionary format
        public Dictionary<string, object> Stats { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0290: Use primary constructure",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "because i said so")]
        public GameStatsEventArgs(Dictionary<string, object> stats)
        {
            Stats = stats ?? [];
        }

        // Helper method to safely get values from the stats dictionary
        // Fixed to handle nullable types properly
        public T? GetValue<T>(string key, T? defaultValue = default)
        {
            if (Stats.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }
    }
}