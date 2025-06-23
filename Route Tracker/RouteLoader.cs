using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Responsible for loading route data from TSV files
    // Parses file content into structured RouteEntry objects
    // Provides access to available route files in the Routes directory
    // ==========MY NOTES==============
    // This loads the route files and converts them to route entries
    // Handles reading the TSV format and finding available route files
    public class RouteLoader
    {
        // ==========FORMAL COMMENT=========
        // Loads and parses a specific route file into RouteEntry objects
        // Processes tab-delimited data with display text, collectible type, and condition values
        // Validates entries and logs diagnostic information during loading
        // ==========MY NOTES==============
        // Reads a TSV file and creates RouteEntry objects from each line
        // Expects each line to have at least 3 columns with the right data
        // Shows debug info in the console to help troubleshoot issues
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822",
        Justification = "it breaks everything")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks everything")]
        public List<RouteEntry> LoadRoute(string filename)
        {
            List<RouteEntry> entries = [];
            string routePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes", filename);

            if (File.Exists(routePath))
            {
                foreach (string line in File.ReadAllLines(routePath))
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length >= 4) // Now checking for at least 4 columns (including coordinates)
                    {
                        string displayText = parts[0].Trim();
                        string collectibleType = parts[1].Trim().ToLowerInvariant();

                        if (int.TryParse(parts[2].Trim(), out int conditionValue))
                        {
                            // Create RouteEntry with proper type and condition
                            RouteEntry entry = new(displayText, collectibleType, conditionValue);

                            // Add coordinates from the fourth column if available
                            if (parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]))
                            {
                                entry.Coordinates = parts[3].Trim();
                            }

                            entries.Add(entry);

                            // Debug output
                            Console.WriteLine($"Loaded: {displayText}, Type: {collectibleType}, Condition: {conditionValue}, Coordinates: {entry.Coordinates}");
                        }
                    }
                }
            }
            return entries;
        }
        // ==========FORMAL COMMENT=========
        // Retrieves a list of available route files in the Routes directory
        // Returns filenames of all TSV files that can be loaded as routes
        // Handles missing directories and access errors gracefully
        // ==========MY NOTES==============
        // Finds all the TSV files in the Routes folder
        // Returns just the filenames without the full path
        // Returns an empty array if the folder doesn't exist or can't be accessed
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0305",
        Justification = "it breaks everything")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks everything")]
        public static string[] GetAvailableRoutes()
        {
            string routeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes");

            if (!Directory.Exists(routeDirectory))
                return [];

            try
            {
                return Directory.GetFiles(routeDirectory, "*.tsv")
       .Select(path => Path.GetFileName(path) ?? string.Empty)
       .ToArray();
            }
            catch
            {
                return [];
            }
        }
    }
}