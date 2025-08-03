using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // Handles game-specific route completion logic
    // Keeps completion logic separate from RouteManager to avoid bloating that class
    // Supports multiple games through polymorphic dispatch
    public static class CompletionManager
    {
        // ==========MY NOTES==============
        // Centralized mappings to eliminate repetitive code and improve maintainability
        private static readonly Dictionary<string, Func<AC4GameStats, string, int, bool>> LocationCheckers = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Chest"] = (stats, location, condition) => stats.LocationChestCounts.TryGetValue(location, out int count) && count >= condition,
            ["Animus Fragment"] = (stats, location, condition) => stats.LocationFragmentCounts.TryGetValue(location, out int count) && count >= condition,
            ["Taverns"] = (stats, location, condition) => stats.LocationTavernCounts.TryGetValue(location, out int count) && count >= condition,
            ["Treasure Map"] = (stats, location, condition) => stats.LocationTreasureMapCounts.TryGetValue(location, out int count) && count >= condition,
            ["Viewpoint"] = (stats, location, condition) => stats.LocationViewpointsCounts.TryGetValue(location, out int count) && count >= condition
        };

        private static readonly Dictionary<string, string> StatKeyMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Story"] = "Story Missions",
            ["Myan Stones"] = "Myan Stones",
            ["Burired Treasure"] = "Buried Treasure",  // Keeping your intentional route file typo -> correct stat name
            ["Assassin Contracts"] = "Assassin Contracts",
            ["Naval Contracts"] = "Naval Contracts",
            ["Letters"] = "Letter Bottles",
            ["Manuscripts"] = "Manuscripts",
            ["Shanty"] = "Music Sheets",
            ["Forts"] = "Forts",
            ["Legendary Ships"] = "Legendary Ships",
            ["Templar Hunts"] = "Templar Hunts",
            ["Upgrades"] = "Hero Upgrades"
        };

        // ==========MY NOTES==============
        // Main entry point for checking completion - delegates to game-specific logic
        // Called by RouteManager.CheckCompletion() to keep existing API intact
        public static bool CheckEntryCompletion(RouteEntry entry, GameStatsEventArgs stats, GameConnectionManager? gameConnectionManager)
        {
            if (string.IsNullOrEmpty(entry.Type))
                return false;

            // Delegate to game-specific completion checker based on connected game
            if (gameConnectionManager?.GameStats is AC4GameStats ac4Stats)
            {
                return CheckAC4Completion(entry, stats, ac4Stats);
            }

            // For future games, add more cases here:
            // if (gameConnectionManager?.GameStats is GoW2018GameStats gowStats)
            // {
            //     return CheckGoWCompletion(entry, stats, gowStats);
            // }

            // Fallback for unknown games
            return false;
        }

        // ==========MY NOTES==============
        // Simplified AC4 completion logic using dictionary mappings
        // Handles both location-based and general stat-based completion
        private static bool CheckAC4Completion(RouteEntry entry, GameStatsEventArgs stats, AC4GameStats ac4Stats)
        {
            string normalizedType = entry.Type.Trim();

            // Handle location-based collectibles using centralized mapping
            if (!string.IsNullOrEmpty(entry.Location) && LocationCheckers.TryGetValue(normalizedType, out var locationChecker))
            {
                bool result = locationChecker(ac4Stats, entry.Location, entry.LocationCondition);
                if (!result)
                {
                    Debug.WriteLine($"Location '{entry.Location}' check failed for {normalizedType} '{entry.Name}' - needed: {entry.LocationCondition}");
                }
                return result;
            }

            // Handle general stat-based completion using centralized mapping
            if (StatKeyMappings.TryGetValue(normalizedType, out string? statKey))
            {
                bool result = stats.GetValue<int>(statKey, 0) >= entry.Condition;
                return result;
            }

            Debug.WriteLine($"Unknown entry type: '{normalizedType}' for entry '{entry.Name}'");
            return false;
        }

        // ==========MY NOTES==============
        // Future method for God of War 2018 completion logic
        // Add this when implementing GoW support
        /*
        private static bool CheckGoWCompletion(RouteEntry entry, GameStatsEventArgs stats, GoW2018GameStats gowStats)
        {
            string normalizedType = entry.Type.Trim();
            
            // Add GoW-specific completion logic here
            return normalizedType switch
            {
                "Raven" or "raven" => stats.GetValue<int>("Ravens", 0) >= entry.Condition,
                "Artifact" or "artifact" => stats.GetValue<int>("Artifacts", 0) >= entry.Condition,
                "Chest" or "chest" => stats.GetValue<int>("Chests", 0) >= entry.Condition,
                _ => false,
            };
        }
        */
    }
}

// not using but may use in future or something idk.
/*
// Get the special activity counts from the game stats
var specialCounts = new Dictionary<string, int>();
if (stats.Stats.TryGetValue("Special Activities", out var specialActivitiesObj) &&
    specialActivitiesObj is Dictionary<string, int> specialActivities)
{
    specialCounts = specialActivities;
}
*/

// not using these but just in case
//"Viewpoint" or "viewpoint" => stats.GetValue<int>("Viewpoints", 0) >= entry.Condition,
//"Chest" or "chest" => stats.GetValue<int>("Chests", 0) >= entry.Condition,
//"Animus Fragment" or "animus fragment" => stats.GetValue<int>("Animus Fragments", 0) >= entry.Condition,
//"Taverns" or "taverns" => stats.GetValue<int>("Taverns", 0) >= entry.Condition,
//"Treasure Map" or "treasure map" => stats.GetValue<int>("Treasure Maps", 0) >= entry.Condition,
//"Modern Day" or "modern day" => stats.GetValue<int>("Modern Day Missions", 0) >= entry.Condition,