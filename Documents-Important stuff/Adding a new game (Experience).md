# Adding a New Game to Route Tracker (Experienced Developer Guide)

**Target Audience:** Experienced C# developers familiar with Windows memory management, P/Invoke, and WinForms development.

## Quick Overview

Route Tracker uses a modular architecture with memory-cached game stats classes that inherit from `GameStatsBase`. Adding a new game requires implementing memory reading logic, updating UI components, and creating route data files.

**Core Components to Modify:**
- Game stats class (inherits `GameStatsBase`)
- `GameConnectionManager` (process detection/connection)
- `SettingsManager` (directory persistence)
- UI forms (`ConnectionWindow`, `GameDirectoryForm`)
- Route file (TSV format)
- `RouteManager.CheckCompletion()` (if new collectible types)

---

## 1. Implement Game Stats Class

Create `{Game}GameStats.cs` inheriting from `GameStatsBase`:
```csharp
public unsafe class GoW2018GameStats : GameStatsBase 
{ 
	private readonly int[] ravenOffsets = { 0x123, 0x456, 0x789 }; 
	private readonly int[] chestOffsets = { 0xABC, 0xDEF, 0x101 };
	private bool isLoading = false;
    private bool isMainMenu = false;

    public GoW2018GameStats(IntPtr processHandle, IntPtr baseAddress) : base(processHandle, baseAddress) 
    { }

    public override Dictionary<string, object> GetStatsAsDictionary()
    {
        // Use ReadWithCache<T>() for 32-bit or ReadWithCache64Bit<T>() for 64-bit games
        int ravens = ReadWithCache64Bit<int>("ravens", (nint)baseAddress, ravenOffsets);
        int chests = ReadWithCache64Bit<int>("chests", (nint)baseAddress, chestOffsets);
    
        // Handle loading/menu detection
        DetectGameStatus(loadingValue, mainMenuValue);
    
        return new Dictionary<string, object>
        {
            ["Ravens"] = ravens,
            ["Chests"] = chests,
            ["Is Loading"] = isLoading,
            ["Is Main Menu"] = isMainMenu,
            ["Game"] = "God of War 2018"
        };
    }

    public override (bool IsLoading, bool IsMainMenu) GetGameStatus()
    {
        return (isLoading, isMainMenu);
    }

    private void DetectGameStatus(int loading, bool mainMenu)
    {
        isMainMenu = mainMenu;
        isLoading = loading >= 256; // Example: loading screen values
    }
}
```

---

**Key Points:**
- `GameStatsBase` provides memory caching via `ReadWithCache<T>()` and `ReadWithCache64Bit<T>()`
- Auto-update system with adaptive timers (500ms active, 1000ms idle)
- Must implement loading/menu detection for auto-save functionality
- Use appropriate Read method based on game architecture (32-bit vs 64-bit)

---

## 2. Update GameConnectionManager

**File:** `GameConnectionManager.cs`

### DetectRunningGame()
```csharp
Dictionary<string, string> gameProcessMap = new() 
{ 
    { "AC4BFSP", "Assassin's Creed 4" }, 
    { "GoW", "God of War 2018" } 
};
```
### InitializeGameStats()
```csharp
gameStats = gameName switch 
{ 
    "Assassin's Creed 4" => new AC4GameStats(processHandle, baseAddress), 
    "God of War 2018" => new GoW2018GameStats(processHandle, baseAddress), 
    _ => throw new NotSupportedException($"Game {gameName} is not supported") 
};
```
### ConnectToGameAsync()
```csharp
if (gameName == "God of War 2018")
{
    currentProcess = "GoW.exe";
}
```

---

## 3. Update SettingsManager

**File:** `SettingsManager.cs`

### Add Settings Property
Add to `Settings.settings`:
```csharp
[UserScopedSetting()] [DefaultSettingValue("")] public string Gow2018Directory 
{ 
    get { return ((string)(this["Gow2018Directory"])); } set { this["Gow2018Directory"] = value; } 
}
```

### Update Methods
```csharp
public string GetGameDirectory(string game) 
{ 
    return game switch 
    { 
        "Assassin's Creed 4" => Settings.Default.AC4Directory, 
        "God of War 2018" => Settings.Default.Gow2018Directory, 
        _ => string.Empty 
    }; 
}

public void SaveDirectory(string selectedGame, string directory) 
{ 
    switch (selectedGame) 
    { 
        case "Assassin's Creed 4": 
            Settings.Default.AC4Directory = directory; 
            break; 
        case "God of War 2018": 
            Settings.Default.Gow2018Directory = directory; 
            break; 
     } 
     Settings.Default.Save(); 
     BackupSettings(); 
}

public List<string> GetGamesWithDirectoriesSet() 
{ 
    var supportedGames = new[] { "Assassin's Creed 4", "God of War 2018" }; 
    return supportedGames.Where(game => !string.IsNullOrEmpty(GetGameDirectory(game))).ToList(); 
}
```

---

## 4. Update UI Components

### ConnectionWindow.cs & GameDirectoryForm.cs
```csharp
gameDropdown.Items.AddRange(["", "Assassin's Creed 4", "God of War 2018"]);

// For GameDirectoryForm.cs Update change handler and save method with new game logic
```

---

## 5. Create Route File

**Format:** TSV (Tab-Separated Values) in `Routes/` folder

**Structure:**
```tsv
Name/Description	Type	Number	Coordinates
```
**Example:** `God of War 2018 100% Route - Main Route.tsv`
```tsv
Raven #1	Raven	1	
Midgard - Wildwoods Nornir Chest #1	Chest	1	
Midgard - River Pass Artifact #1	Artifact	1	
Midgard - Lookout Tower
```

**Auto-numbering formula (Google Sheets):**
```excel
=COUNTIF($B$2:B2, B2)
```

---

## 6. Update RouteManager (If Needed)

**File:** `RouteManager.cs` - `CheckCompletion()` method
```csharp
return normalizedType switch 
{ 
    "viewpoint" => stats.GetValue<int>("Viewpoints", 0) >= entry.Condition, 
    "raven" => stats.GetValue<int>("Ravens", 0) >= entry.Condition, 
    "artifact" => stats.GetValue<int>("Artifacts", 0) >= entry.Condition, 
    "chest" => stats.GetValue<int>("Chests", 0) >= entry.Condition, 
    _ => false, 
};
```

**Note:** Only required if introducing new collectible types not handled by existing logic.

---

## 7. Update Stats Display (Optional)

**File:** `RouteHelpers.cs` - `BuildStatsText()` method
```csharp
if (statsDict.ContainsKey("Ravens")) 
{ 
    baseStats += $"Ravens Collected: 
    {
        statsDict.GetValueOrDefault("Ravens", 0)}\n" + 
        $"Artifacts Found: {statsDict.GetValueOrDefault("Artifacts", 0)}\n" + 
        $"Chests Opened: {statsDict.GetValueOrDefault("Chests", 0)}\n"; 
     }
}
```

---

## Architecture Notes

### Memory Management
- `GameStatsBase` provides automatic memory caching with 500ms duration
- Adaptive update timers (500ms active, 1000ms idle) based on stat changes
- Built-in cache invalidation and performance optimization

### Auto-Save System
- Requires `IsLoading` and `IsMainMenu` detection from `GetGameStatus()`
- Progress saved on loading screens and menu transitions
- Cycling backup system (1-10 numbered saves)

### Enhanced Features
- Settings backup to AppData with restore functionality
- Multiple layout modes (Normal, Compact, Mini, Overlay)
- Advanced filtering and search with history
- Global hotkey support with advanced/normal modes

### Logging Integration
- Use `LoggingSystem.LogInfo()`, `LoggingSystem.LogWarning()`, `LoggingSystem.LogError()`
- Automatic error reporting with user consent
- Developer device detection for testing

---

## Testing Checklist

- [ ] Process detection works in `ConnectionWindow`
- [ ] Memory reads return expected values (use debugger/logging)
- [ ] Route file loads correctly (TSV format validation)
- [ ] Completion tracking works with memory values
- [ ] Auto-save triggers on loading/menu detection
- [ ] Settings persistence across app restarts
- [ ] Stats display updates in real-time
- [ ] Game directory setting saves correctly

---

## Common Patterns

**32-bit vs 64-bit Games:**
- Use `ReadWithCache<T>()` for 32-bit pointer chains
- Use `ReadWithCache64Bit<T>()` for 64-bit pointer chains

**Loading Detection:**
- Boolean: Direct comparison
- Integer: Range comparison (e.g., `>= 256`)
- State machines: Multiple value checking

**Collectible Counting:**
- Direct memory reads for simple counters
- Range-based calculations for complex systems
- Cache keys should be unique per stat type

**Error Handling:**
- Memory reads return `default(T)` on failure
- Use logging system for debugging
- Implement fallback logic where possible

**Stuff u could improve if u want to**
- Most of this code aka 90% of the program was ai generated so it's not the best code
- so if you see any areas for improvements feel free to improve it
- or any systems that are already in place that could be improved or rewritten feel free to do so
- just make sure to test it before pushing it to the main branch
- If you see any bugs or issues feel free to fix them
- if you see any performance issues feel free to improve them
- if you want to add any new features feel free to do so just mention it in the pull request
- For pull request please make a list of all changes ie like
    - Added new game support for God of War 2018
    - fixed bugs in this/these file(s)
    - Add new feature(s)
        - Explain what the feature is and does and how it works
    - Optimized code in this/these file(s)
- If you have any questions feel free to ask me on discord or github ie make a issue or something

**Optional:**
- If you want to head over to the README.md file and add the new game to the list of supported games
- Add your name(discord, github, or wahtever you want(as long as its not inappropirate) to the credits section in the README.md file near the bottom
