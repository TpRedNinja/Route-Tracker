# How to Add a New Game to Route Tracker (Updated Guide)

This guide walks you through adding a new game—using God of War 2018 as an example—to Route Tracker.

It covers everything: memory pointers, code changes, UI, settings, and route file structure, reflecting the current architecture after code reorganization.

No prior experience required!

---

🎯 1. Gather What You Need

Before you start, make sure you have:

- Game executable name (for God of War 2018, it's GoW.exe)

- Memory pointers/offsets for each collectible or stat you want to track (e.g., ravens, chests, artifacts)

- Route file data listing all the items/collectibles you want to track (see step 7 for details)

💡 Use memory scanning tools like Cheat Engine to find memory addresses for collectibles.

---

🛠️ 2. Create the Game Stats Class

File: Route Tracker\GoW2018GameStats.cs

This class reads the game's memory and provides stats to the rest of the program. It inherits from the enhanced `GameStatsBase` class which now includes automatic caching, adaptive timers, and improved memory management.
```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Route_Tracker
{
    public unsafe class GoW2018GameStats : GameStatsBase
    {
        // Example: memory offsets for collectibles (replace with real values)
        private readonly int[] ravenOffsets = { 0x123, 0x456, 0x789 };
        private readonly int[] chestOffsets = { 0xABC, 0xDEF, 0x101 };
        private readonly int[] artifactOffsets = { 0x222, 0x333, 0x444 };

        // Pre-calculated base address for collectibles
        private readonly nint collectiblesBaseAddress;

        public GoW2018GameStats(IntPtr processHandle, IntPtr baseAddress)
            : base(processHandle, baseAddress)
        {
            // Set up base addresses for memory reading
            this.collectiblesBaseAddress = (nint)baseAddress + 0x1000; // example offset
            Debug.WriteLine("GoW2018GameStats initialized");
        }

        // Override this method to provide game-specific stats using the enhanced caching system
        public override Dictionary<string, object> GetStatsAsDictionary()
        {
            // Use ReadWithCache for better performance (inherited from GameStatsBase)
            int ravens = ReadWithCache<int>("ravens", (nint)baseAddress, ravenOffsets);
            int chests = ReadWithCache<int>("chests", (nint)baseAddress, chestOffsets);
            int artifacts = ReadWithCache<int>("artifacts", (nint)baseAddress, artifactOffsets);

            return new Dictionary<string, object>
            {
                ["Ravens"] = ravens,
                ["Chests"] = chests,
                ["Artifacts"] = artifacts,
                ["Game"] = "God of War 2018",
                // Add more stats as needed
            };
        }

        public override (bool IsLoading, bool IsMainMenu) GetGameStatus()
        {
            // Implement loading screen and main menu detection
            // For now, return false for both until you implement detection
            return (false, false);
        }
    }
}
````

What's happening here?

- You define memory pointers for each collectible/stat as int[] arrays

- GetStatsAsDictionary() uses the enhanced ReadWithCache method for better performance

- GetGameStatus() detects loading screens or main menu (implement based on your game's memory structure)

- The base class now handles automatic updates, caching, and performance optimization

---

🔗 3. Update GameConnectionManager

File: Route Tracker\GameConnectionManager.cs

The connection manager has been enhanced but the core integration points remain the same.

3.1 Update the DetectRunningGame() method
```csharp
public string DetectRunningGame()
{
    // Define known game processes and their friendly names
    Dictionary<string, string> gameProcessMap = new()
    {
        { "AC4BFSP", "Assassin's Creed 4" },
        { "GoW", "God of War 2018" }
        // Add more games here as needed
    };

    // Check for running processes that match our supported games
    foreach (var process in Process.GetProcesses())
    {
        try
        {
            // Check if this process name matches any of our supported games
            foreach (var game in gameProcessMap)
            {
                if (process.ProcessName.Contains(game.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return game.Value; // Return the friendly game name
                }
            }
        }
        catch
        {
            // Skip any processes we can't access
            continue;
        }
    }

    return string.Empty; // No matching game found
}
```
3.2 Update the InitializeGameStats() method
```csharp
public void InitializeGameStats(string gameName)
{
    if (processHandle != IntPtr.Zero && baseAddress != IntPtr.Zero)
    {
        // Create the correct stats object based on game
        gameStats = gameName switch
        {
            "Assassin's Creed 4" => new AC4GameStats(processHandle, baseAddress),
            "God of War 2018" => new GoW2018GameStats(processHandle, baseAddress),
            _ => throw new NotSupportedException($"Game {gameName} is not supported")
        };

        gameStats.StatsUpdated += OnGameStatsUpdated;
        gameStats.StartUpdating();
    }
}
```
3.3 Update the ConnectToGameAsync() method
```csharp
public async Task<bool> ConnectToGameAsync(string gameName, bool autoStart = false)
{
    // Set the correct process name based on game selection
    if (gameName == "Assassin's Creed 4")
        currentProcess = "AC4BFSP.exe";
    else if (gameName == "God of War 2018")
        currentProcess = "GoW.exe";
    else
        return false; // Invalid game selection

    // Auto-start the game if requested and not already running
    if (autoStart)
    {
        if (!IsProcessRunning(currentProcess))
        {
            StartGame(currentProcess);
            await WaitForGameToStartAsync();
        }
    }

    // Attempt to connect to the game process
    Connect();

    // Initialize game stats if connection was successful
    if (processHandle != IntPtr.Zero)
    {
        InitializeGameStats(gameName);
        return true;
    }

    return false;
}
```
Why these changes?

This tells the program which executable to look for, how to start the game, and which stats class to use when connecting.

---

⚙️ 4. Update SettingsManager

File: Route Tracker\SettingsManager.cs

The settings system has been enhanced with backup functionality, but the core game directory methods remain the same.

4.1 Update GetGameDirectory() method
```chsarp
public string GetGameDirectory(string game)
{
    if (game == "Assassin's Creed 4")
        return Settings.Default.AC4Directory;
    else if (game == "God of War 2018")
        return Settings.Default.Gow2018Directory;
    return string.Empty;
}
```
4.2 Update SaveDirectory() method
```chsarp
public void SaveDirectory(string selectedGame, string directory)
{
    if (selectedGame == "Assassin's Creed 4")
    {
        Settings.Default.AC4Directory = directory;
    }
    else if (selectedGame == "God of War 2018")
    {
        Settings.Default.Gow2018Directory = directory;
    }
    Settings.Default.Save();
    
    // Enhanced: Automatically create backup after saving
    BackupSettings();
}
```
4.3 Update GetGamesWithDirectoriesSet() method
```chsarp
public List<string> GetGamesWithDirectoriesSet()
{
    var games = new List<string>();
    var supportedGames = new[] { "Assassin's Creed 4", "God of War 2018" };
    foreach (var game in supportedGames)
    {
        if (!string.IsNullOrEmpty(GetGameDirectory(game)))
            games.Add(game);
    }
    return games;
}
```

Why these changes?

The program needs to know where your game is installed to launch it and attach to its process. The enhanced settings manager now automatically backs up settings when changed.

---

🖥️ 5. Update UI Elements

The UI system has been reorganized into helper classes. You'll need to update multiple files:

5.1 Update ConnectionWindow

File: Route Tracker\ConnectionWindow.cs

This is the new dedicated connection window that replaced the old dropdown system.
```chsarp
private void InitializeCustomComponents()
{
    this.Text = "Connect to Game";
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    this.MaximizeBox = false;
    this.MinimizeBox = false;
    this.StartPosition = FormStartPosition.CenterParent;
    this.Width = 400;
    this.Height = 180;

    connectionLabel = new Label
    {
        Text = "Not connected",
        AutoSize = true,
        Top = 20,
        Left = 20
    };
    this.Controls.Add(connectionLabel);

    gameDropdown = new ComboBox
    {
        Left = 20,
        Top = 50,
        Width = 200,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    gameDropdown.Items.AddRange(["", "Assassin's Creed 4", "God of War 2018"]); // Add your game here
    gameDropdown.SelectedIndex = 0;
    this.Controls.Add(gameDropdown);

    // ... rest of the method remains the same
}
```

5.2 Update GameDirectoryForm

File: Route Tracker\GameDirectoryForm.cs

Update the GameDropdown_SelectedIndexChanged method and SaveDirectory method.
```chsarp
private void GameDropdown_SelectedIndexChanged(object? sender, EventArgs e)
{
    string selectedGame = gameDropdown.SelectedItem?.ToString() ?? string.Empty;
    if (selectedGame == "Assassin's Creed 4")
    {
        directoryTextBox.Text = Settings.Default.AC4Directory;
    }
    else if (selectedGame == "God of War 2018")
    {
        directoryTextBox.Text = Settings.Default.Gow2018Directory;
    }
}
```

5.3 Update GameDirectoryForm - SaveDirectory

File: Route Tracker\GameDirectoryForm.cs

The settings menu is now managed in its own class. The auto-start dropdown automatically updates when you add games, so no changes are needed here unless you want to add game-specific settings.
```chsarp
private void SaveDirectory()
{
    string selectedGame = gameDropdown.SelectedItem?.ToString() ?? string.Empty;
    if (selectedGame == "Assassin's Creed 4")
    {
        Settings.Default.AC4Directory = directoryTextBox.Text;
    }
    else if (selectedGame == "God of War 2018")
    {
        Settings.Default.Gow2018Directory = directoryTextBox.Text;
    }
    Settings.Default.Save();

    DirectoryChanged?.Invoke(this, EventArgs.Empty);
}
```

Why these changes?

The UI has been reorganized for better maintainability. The ConnectionWindow provides a cleaner connection experience, and settings are managed centrally.

---

🧩 6. Add Settings Property

In your application settings (Settings.settings or Settings.Designer.cs), add a new property for your game's directory.
```csharp
[global::System.Configuration.UserScopedSettingAttribute()]
[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
[global::System.Configuration.DefaultSettingValueAttribute("")]
public string Gow2018Directory {
    get {
        return ((string)(this["Gow2018Directory"]));
    }
    set {
        this["Gow2018Directory"] = value;
    }
}
```

Alternative: You can add this through Visual Studio's Settings editor:

- Name: Gow2018Directory
- Type: string
- Scope: User
- Value: (empty)

---

📄 7. Create a Route File

Folder: Routes/

File name: God of War 2018 100 % Route - Main Route.tsv

The route file format remains the same, but now supports enhanced completion tracking.

File Structure

Each line is a tab-separated value (TSV) with these columns:

| Name/Hint         | Type        | Number | Coordinates (Optional)   |
|-------------------|-------------|--------|-------------------------|
| Raven #1          | Raven       | 1      | Midgard - Wildwoods     |
| Nornir Chest #1   | Chest       | 1      | Midgard - River Pass    |
| Artifact #1       | Artifact    | 1      | Midgard - Lookout Tower |

Example File Content:
```tsv
Raven #1	Raven	1	Midgard - Wildwoods 
Nornir Chest #1	Chest	1	Midgard - River Pass 
Artifact #1	Artifact	1	Midgard - Lookout Tower 
Legendary Chest #1	Chest	2	Midgard - Witch's Cave 
Raven #2	Raven	2	Midgard - The River Pass
````

Column Definitions

- Name/Hint: What the player is looking for (displayed in the UI)
- Type: The type of collectible (must match what your code checks for)
- Number: Unique ID for that collectible (used for progress tracking)
- Coordinates: (Optional) Location information for the player

The enhanced route system now supports:
- Automatic progress saving with cycling backups
- Better prerequisite tracking
- Enhanced filtering and search capabilities

---

🧭 8. Update RouteManager (If Needed)

File: Route Tracker\RouteManager.cs

The RouteManager has been significantly enhanced but the core completion checking logic remains in the same place. If your new game has unique collectible types, update the CheckCompletion method.
```csharp
private bool CheckCompletion(RouteEntry entry, GameStatsEventArgs stats)
{
    if (string.IsNullOrEmpty(entry.Type))
        return false;

    // Enforce prerequisite for ALL entries
    if (entry.Prerequisite != null && !entry.Prerequisite.IsCompleted)
        return false;

    string normalizedType = entry.Type.Trim();

    return normalizedType switch
    {
        // Assassin's Creed 4 types
        "Viewpoint" or "viewpoint" => stats.GetValue<int>("Viewpoints", 0) >= entry.Condition,
        "Chest" or "chest" => stats.GetValue<int>("Chests", 0) >= entry.Condition,
        
        // God of War 2018 types
        "Raven" or "raven" => stats.GetValue<int>("Ravens", 0) >= entry.Condition,
        "Artifact" or "artifact" => stats.GetValue<int>("Artifacts", 0) >= entry.Condition,
        
        // Add more types as needed
        _ => false,
    };
}
```
Why this change?

This function matches the route file's "Type" column to the correct stat in your code, enabling automatic completion detection. The enhanced RouteManager now includes:
- Cycling autosave system with numbered backups
- Better file searching capabilities
- Enhanced completion tracking

---

📊 9. Update Stats Display (If Needed)

File: Route Tracker\RouteHelpers.cs

The stats display system has been reorganized into RouteHelpers. If you want your new game's stats to appear in the stats window, update the BuildStatsText method to include your game's specific statistics.
```csharp
private static string BuildStatsText(Dictionary<string, object> statsDict)
{
    string baseStats = $"Completion Percentage: {statsDict.GetValueOrDefault("Completion Percentage", 0)}%\n" +
                      $"Completion Percentage Exact: {statsDict.GetValueOrDefault("Exact Percentage", 0):F2}%\n";

    // Add AC4 stats if present
    if (statsDict.ContainsKey("Viewpoints"))
    {
        baseStats += $"Viewpoints Completed: {statsDict.GetValueOrDefault("Viewpoints", 0)}\n" +
                    $"Myan Stones Collected: {statsDict.GetValueOrDefault("Myan Stones", 0)}\n" +
                    $"Buried Treasure Collected: {statsDict.GetValueOrDefault("Buried Treasure", 0)}\n" +
                    $"AnimusFragments Collected: {statsDict.GetValueOrDefault("Animus Fragments", 0)}\n" +
                    $"AssassinContracts Completed: {statsDict.GetValueOrDefault("Assassin Contracts", 0)}\n" +
                    $"NavalContracts Completed: {statsDict.GetValueOrDefault("Naval Contracts", 0)}\n" +
                    $"LetterBottles Collected: {statsDict.GetValueOrDefault("Letter Bottles", 0)}\n" +
                    $"Manuscripts Collected: {statsDict.GetValueOrDefault("Manuscripts", 0)}\n" +
                    $"Music Sheets Collected: {statsDict.GetValueOrDefault("Music Sheets", 0)}\n" +
                    $"Forts Captured: {statsDict.GetValueOrDefault("Forts", 0)}\n" +
                    $"Taverns unlocked: {statsDict.GetValueOrDefault("Taverns", 0)}\n" +
                    $"Total Chests Collected: {statsDict.GetValueOrDefault("Chests", 0)}\n" +
                    $"Story Missions Completed: {statsDict.GetValueOrDefault("Story Missions", 0)}\n" +
                    $"Templar Hunts Completed: {statsDict.GetValueOrDefault("Templar Hunts", 0)}\n" +
                    $"Legendary Ships Defeated: {statsDict.GetValueOrDefault("Legendary Ships", 0)}\n" +
                    $"Treasure Maps Collected: {statsDict.GetValueOrDefault("Treasure Maps", 0)}\n";
    }

    // Add God of War stats if present
    if (statsDict.ContainsKey("Ravens"))
    {
        baseStats += $"Ravens Collected: {statsDict.GetValueOrDefault("Ravens", 0)}\n" +
                    $"Artifacts Found: {statsDict.GetValueOrDefault("Artifacts", 0)}\n" +
                    $"Chests Opened: {statsDict.GetValueOrDefault("Chests", 0)}\n";
    }

    return baseStats;
}
```
---

🧪 10. Testing Your Implementation

Step-by-Step Testing

1. Build and run the application
2. Open Settings > Game Directory
3. Select "God of War 2018" and set its installation directory
4. Start God of War 2018
5. In Route Tracker, click "Connect to Game" and use the new ConnectionWindow
6. Verify that:

- Stats are updating in the Game Stats window
- Route entries load from your TSV file
- Items are marked complete when you collect them in-game
- Progress is automatically saved with cycling backups
- The enhanced UI features work correctly

Debugging Checklist

- [ ] Game executable name matches exactly (GoW.exe)
- [ ] Memory addresses are correct for your game version
- [ ] Route file is properly formatted (tab-separated, not spaces)
- [ ] Collectible types in route file match the switch statement
- [ ] Settings directory is set correctly
- [ ] ConnectionWindow shows your game in the dropdown
- [ ] Autosave system creates backup files correctly

---

🏗️ 11. Understanding the New Architecture

The codebase has been reorganized for better maintainability:

**Helper Classes:**
- **RouteHelpers.cs**: Route data management, filtering, game connection, and window management
- **MainFormHelpers.cs**: UI creation, context menus, and hotkey processing
- **SettingsMenuManager.cs**: Settings menu creation and management
- **LayoutManager.cs**: UI layout management for different modes

**Enhanced Features:**
- **Memory Caching**: GameStatsBase now includes automatic caching for better performance
- **Adaptive Timers**: Update frequency adjusts based on player activity
- **Cycling Backups**: Progress saves with numbered backups (1-10) that cycle
- **Settings Backup**: Automatic settings backup to AppData with restore functionality
- **Enhanced UI**: Multiple layout modes, better connection window, improved filtering

**Key Files to Update:**
1. **GameStatsBase**: Your game stats class (inherits enhanced features)
2. **GameConnectionManager**: Game detection and connection logic
3. **SettingsManager**: Game directory management
4. **ConnectionWindow**: Game selection and connection UI
5. **GameDirectoryForm**: Directory selection UI
6. **RouteManager**: Route completion logic (if needed)
7. **RouteHelpers**: Stats display (if desired)

---

✅ 12. Complete Implementation Checklist
- [ ] Create GoW2018GameStats.cs with proper memory reading
- [ ] Add all pointer offsets for collectibles/stats
- [ ] Update GameConnectionManager.DetectRunningGame()
- [ ] Update GameConnectionManager.InitializeGameStats()
- [ ] Update GameConnectionManager.ConnectToGameAsync()
- [ ] Update SettingsManager.GetGameDirectory()
- [ ] Update SettingsManager.SaveDirectory()
- [ ] Update SettingsManager.GetGamesWithDirectoriesSet()
- [ ] Add game to ConnectionWindow dropdown
- [ ] Add game to GameDirectoryForm dropdown
- [ ] Add Gow2018Directory setting property
- [ ] Create route TSV file with proper format
- [ ] Update RouteManager.CheckCompletion() if needed
- [ ] Update RouteHelpers.BuildStatsText() if desired
- [ ] Test the complete workflow with enhanced features
- [ ] Verify cycling autosave system works
- [ ] Test connection window functionality
- [ ] Verify settings backup system works

---

🗂️ 13. Route File Format Reference

AC4 vs GoW Example Comparison

Assassin's Creed 4:
```tsv
Havana Viewpoint 1	Viewpoint	1	
Havana Havana Chest 1	
Chest	1	Havana
```

God of War 2018:
```tsv
Raven #1	Raven	1	Midgard - Wildwoods 
Nornir Chest #1	Chest	1	Midgard - River Pass
```

Important Notes

- Use tabs (not spaces) to separate columns
- The "Type" column must exactly match your switch statement cases
- Numbers should be sequential for each type of collectible
- Coordinates are optional but helpful for players

---

🚀 14. Advanced Features (Optional)

The enhanced architecture now supports:

**Memory Optimization:**
- ReadWithCache() for efficient memory access
- Automatic cache invalidation
- Performance monitoring and adaptive updates

**Enhanced Backup System:**
- Cycling progress backups (1-10) with automatic rotation
- Settings backup to AppData with restore functionality
- Automatic backup creation on changes

**Improved UI:**
- Multiple layout modes (Normal, Compact, Mini, Overlay)
- Enhanced filtering and search capabilities
- Better connection management with dedicated window
- Right-click context menus for common actions

**Advanced Route Features:**
- Enhanced prerequisite system
- Better completion tracking
- Improved file searching with fallback locations
- Support for game state transitions (loading, menu, gameplay)

---

🛑 15. Troubleshooting Common Issues

**Game Not Detected**
- Verify the exact process name using Task Manager
- Check ConnectionWindow dropdown includes your game
- Ensure DetectRunningGame() includes your process mapping

**Stats Not Updating**
- Confirm memory addresses are correct for your game version
- Check if ReadWithCache() calls are properly implemented
- Verify GameStatsBase inheritance is correct
- Use Debug.WriteLine() to log stats retrieval

**Route File Not Loading**
- Check file format (TSV with tabs, not spaces)
- Verify file is in the correct Routes/ folder
- Ensure RouteManager.CheckCompletion() includes your collectible types
- Test with the enhanced file searching system

**Connection Issues**
- Use the new ConnectionWindow instead of old dropdown system
- Check Settings > Game Directory is set correctly
- Verify GameConnectionManager updates are complete
- Run Route Tracker as administrator if needed

**UI Issues**
- Ensure all helper classes are updated
- Check that SettingsMenuManager includes your game
- Verify ConnectionWindow shows your game option
- Test different layout modes work correctly

---

🎉 16. Congratulations!

You've successfully added God of War 2018 to Route Tracker using the enhanced architecture. The same process can be applied to add any other game with the appropriate memory addresses and route data.

**New Benefits:**
- Automatic memory caching for better performance
- Cycling backup system protects your progress
- Enhanced UI with multiple layout modes
- Better connection management
- Automatic settings backup and restore
- Improved filtering and search capabilities

For questions or issues, check the existing code comments or create an issue on the project's GitHub repository.
