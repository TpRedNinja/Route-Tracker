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
        // Logs diagnostic info to help troubleshoot issues
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1822",
        Justification = "it breaks everything")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks everything")]
        public List<RouteEntry> LoadRoute(string filename)
        {
            List<RouteEntry> entries = [];
            string routePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes", filename);

            try
            {
                if (File.Exists(routePath))
                {
                    LoggingSystem.LogInfo($"Loading route file: {filename}");

                    foreach (string line in File.ReadAllLines(routePath))
                    {
                        string[] parts = line.Split('\t');
                        if (parts.Length >= 6)
                        {
                            string displayText = parts[0].Trim();
                            string collectibleType = parts[1].Trim().ToLowerInvariant();

                            bool conditionParsed = int.TryParse(parts[2].Trim(), out int conditionValue);
                            if (!conditionParsed)
                            {
                                LoggingSystem.LogWarning($"Invalid condition value in route entry: {line}");
                                continue;
                            }

                            string coordinates = parts[3].Trim();
                            string location = parts[4].Trim();

                            bool locationConditionParsed = int.TryParse(parts[5].Trim(), out int locationCondition);
                            if (!locationConditionParsed)
                            {
                                LoggingSystem.LogWarning($"Invalid location condition value in route entry: {line}");
                                continue;
                            }

                            RouteEntry entry = new(displayText, collectibleType, conditionValue, location, locationCondition);

                            if (!string.IsNullOrWhiteSpace(coordinates))
                            {
                                entry.Coordinates = coordinates;
                            }

                            entries.Add(entry);
                        }
                        else
                        {
                            LoggingSystem.LogWarning($"Invalid route entry format (insufficient columns): {line}");
                        }
                    }

                    LoggingSystem.LogInfo($"Successfully loaded {entries.Count} route entries from {filename}");
                }
                else
                {
                    LoggingSystem.LogError($"Route file not found: {routePath}");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError($"Error loading route file {filename}", ex);
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