using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Manages search history for the search functionality
    // Handles loading, saving, and managing recent search terms
    // Stores history in JSON format in the Route Tracker json files folder
    // ==========MY NOTES==============
    // Keeps track of what the user has searched for recently
    // Saves to a separate folder from settings as requested
    public class SearchHistoryManager
    {
        private readonly string jsonFilesFolder;
        private readonly string historyFilePath;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public SearchHistoryManager()
        {
            // Create the "Route Tracker json files" folder in the backup directory
            jsonFilesFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RouteTracker",
                "Route Tracker json files"
            );
            historyFilePath = Path.Combine(jsonFilesFolder, "History.json");
            
            // Ensure directory exists
            Directory.CreateDirectory(jsonFilesFolder);
        }

        // ==========MY NOTES==============
        // Adds a search term to the history if it's not empty or duplicate
        // Moves existing duplicates to the top of the list
        public void AddSearchHistory(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            searchTerm = searchTerm.Trim();
            var history = LoadSearchHistory();

            // Remove existing duplicate if it exists
            history.RemoveAll(h => h.Equals(searchTerm, StringComparison.OrdinalIgnoreCase));
            
            // Add to the top of the list
            history.Insert(0, searchTerm);

            SaveSearchHistory(history);
        }

        // ==========MY NOTES==============
        // Loads the search history from the JSON file
        // Returns empty list if file doesn't exist or has errors
        public List<string> LoadSearchHistory()
        {
            if (!File.Exists(historyFilePath))
                return [];

            try
            {
                string json = File.ReadAllText(historyFilePath);
                return JsonSerializer.Deserialize<List<string>>(json) ?? [];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading search history: {ex.Message}");
                return [];
            }
        }

        // ==========MY NOTES==============
        // Saves the search history to the JSON file
        private void SaveSearchHistory(List<string> history)
        {
            try
            {
                string json = JsonSerializer.Serialize(history, JsonOptions);
                File.WriteAllText(historyFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving search history: {ex.Message}");
            }
        }

        // ==========MY NOTES==============
        // Gets the most recent search term (first in the list)
        // Returns empty string if no history exists
        public string GetLastSearchTerm()
        {
            var history = LoadSearchHistory();
            return history.Count > 0 ? history[0] : string.Empty;
        }

        // ==========MY NOTES==============
        // Gets the folder path where JSON files are stored
        public string GetJsonFilesFolder()
        {
            return jsonFilesFolder;
        }
    }
}