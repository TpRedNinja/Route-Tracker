namespace Route_Tracker
{
    public class RouteEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Condition { get; set; }
        public string Coordinates { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int LocationCondition { get; set; }
        public string? ConditionType { get; set; } // e.g., "Viewpoints", "Myan", etc.
        public int? ConditionValue { get; set; } // The value needed for completion
        public int Id { get; set; } // add unique ID for stable identification
        public bool IsSkipped { get; set; }
        public bool IsCompleted { get; set; } = false;
        public RouteEntry? Prerequisite { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0290",
        Justification = "it breaks everything")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks everything")]
        public RouteEntry(string name, string type = "", int condition = 0, string location = "", int locationCondition = 0, int id = 0)
        {
            Name = name;
            Type = type;
            Condition = condition;
            Location = location;
            LocationCondition = locationCondition;
            Id = id;
            IsCompleted = false;
            IsSkipped = false;
        }

        public string DisplayText
        {
            get
            {
                // If coordinates exist and are not "None", append them to the name in brackets
                if (!string.IsNullOrEmpty(Coordinates) && !Coordinates.Equals("None", StringComparison.OrdinalIgnoreCase))
                    return $"{Name}, {Coordinates}";
                else
                    return Name;
            }
        }
    }
}