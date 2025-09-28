namespace Route_Tracker
{
    public class RouteLoader
    {
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

        public static string[] GetAvailableRoutes()
        {
            string routeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes");

            if (!Directory.Exists(routeDirectory))
                return [];

            try
            {
                return [.. Directory.GetFiles(routeDirectory, "*.tsv").Select(path => Path.GetFileName(path) ?? string.Empty)];
            }
            catch
            {
                return [];
            }
        }
    }
}