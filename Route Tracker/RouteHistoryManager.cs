using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Manages history of downloaded routes for backup and restore functionality
    // Handles storage location, history tracking, and integration with backup system
    // ==========MY NOTES==============
    // Keeps track of all downloaded routes so they can be restored from backup
    // Uses the same folder structure as settings backups
    public class RouteHistoryManager
    {
        private readonly string downloadFolder;
        private readonly string historyFilePath;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public RouteHistoryManager()
        {
            // Use same backup location as settings but separate folder for routes
            downloadFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RouteTracker",
                "DownloadedRoutes"
            );
            historyFilePath = Path.Combine(downloadFolder, "download_history.json");
            
            // Ensure directory exists
            Directory.CreateDirectory(downloadFolder);
        }

        public string GetDownloadPath(string filename)
        {
            return Path.Combine(downloadFolder, filename);
        }

        public void AddDownloadHistory(string url, string filename, string filePath)
        {
            var history = LoadDownloadHistory();
            
            var entry = new DownloadHistoryEntry
            {
                Url = url,
                Filename = filename,
                FilePath = filePath,
                DownloadDate = DateTime.Now
            };

            // Remove any existing entry with same filename
            history.RemoveAll(h => h.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase));
            
            // Add new entry
            history.Add(entry);
            
            // Keep only last 50 entries
            if (history.Count > 50)
            {
                history = [.. history.OrderByDescending(h => h.DownloadDate).Take(50)];
            }

            SaveDownloadHistory(history);
        }

        public List<DownloadHistoryEntry> LoadDownloadHistory()
        {
            if (!File.Exists(historyFilePath))
                return [];

            try
            {
                string json = File.ReadAllText(historyFilePath);
                return JsonSerializer.Deserialize<List<DownloadHistoryEntry>>(json) ?? [];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading download history: {ex.Message}");
                return [];
            }
        }

        private void SaveDownloadHistory(List<DownloadHistoryEntry> history)
        {
            try
            {
                string json = JsonSerializer.Serialize(history, JsonOptions);
                File.WriteAllText(historyFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving download history: {ex.Message}");
            }
        }

        public List<string> GetDownloadedRouteFiles()
        {
            var history = LoadDownloadHistory();
            return [.. history.Where(h => File.Exists(h.FilePath)).Select(h => h.FilePath)];
        }

        public void CleanupOrphanedFiles()
        {
            var history = LoadDownloadHistory();
            var validEntries = new List<DownloadHistoryEntry>();

            foreach (var entry in history)
            {
                if (File.Exists(entry.FilePath))
                {
                    validEntries.Add(entry);
                }
            }

            if (validEntries.Count != history.Count)
            {
                SaveDownloadHistory(validEntries);
            }
        }

        public string GetDownloadFolder()
        {
            return downloadFolder;
        }
    }

    public class DownloadHistoryEntry
    {
        public string Url { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime DownloadDate { get; set; }
    }
}