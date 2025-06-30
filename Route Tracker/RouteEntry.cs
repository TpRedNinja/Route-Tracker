using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Represents a single entry in a route file
    // Contains all properties needed to track and display a route step
    // Maintains completion state during runtime
    // ==========MY NOTES==============
    // This is the data model for each line in the route file
    // Holds all the info about what needs to be done and whether it's complete
    public class RouteEntry
    {
        // ==========FORMAL COMMENT=========
        // Properties representing route entry data from TSV file
        // Includes descriptive information, conditions for completion, and location data
        // Provides storage for both static file data and dynamic runtime state
        // ==========MY NOTES==============
        // These store all the details about each route item
        // Some come directly from the file, others are calculated during runtime
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Condition { get; set; }
        public string Coordinates { get; set; } = string.Empty;
        public string LocationCondition { get; set; } = string.Empty;
        public string? ConditionType { get; set; } // e.g., "Viewpoints", "Myan", etc.
        public int? ConditionValue { get; set; } // The value needed for completion
        public int Id { get; set; } // add unique ID for stable identification

        // add skipped flag
        public bool IsSkipped { get; set; }

        // Runtime state
        public bool IsCompleted { get; set; } = false;

        public RouteEntry? Prerequisite { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0290",
        Justification = "it breaks everything")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
        Justification = "it breaks everything")]
        public RouteEntry(string name, string type = "", int condition = 0, int id = 0)
        {
            Name = name;
            Type = type;
            Condition = condition;
            Id = id;
            IsCompleted = false;
            IsSkipped = false;
        }

        // ==========FORMAL COMMENT=========
        // Provides formatted display text combining name and coordinates
        // Handles cases where coordinates may or may not be present
        // Used for showing route entries in the UI
        // ==========MY NOTES==============
        // Creates the text shown in the route list
        // Adds coordinates in brackets if they exist
        // Makes it easier to find locations in-game
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