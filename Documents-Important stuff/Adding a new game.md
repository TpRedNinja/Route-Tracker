# How to Add a New Game to Route Tracker (Beginner-Friendly C# Guide)

This guide will walk you through adding a new game (using God of War 2018 as an example) to Route Tracker.

**Who is this for?**  
If you know how to find game memory addresses and work with pointers (like in Cheat Engine or autosplitters), but you’re new to C# or Visual Studio, this guide is for you.

You’ll learn how to:
- Define your game’s memory pointers in C#
- Update the code to recognize your game
- Add your game to the UI and settings
- Create a route file for your collectibles
- Test everything in Visual Studio

**No prior C# experience required!**  
You’ll get short explanations of C# concepts as you see them, so you can focus on the memory logic you already know.

---

## Before You Start

**What you’ll need:**
- Visual Studio 2022 (Community Edition is fine)
- The Route Tracker project opened in Visual Studio
- Your game’s executable name (e.g., `GoW.exe`)
- Memory pointers/offsets for each collectible or stat you want to track (e.g., ravens, chests, artifacts)
- A list of all items/collectibles you want to track (for the route file, explained later)

**Tip:**  
If you haven’t used Visual Studio before:
- Open the solution file (`.sln`) to load the project.
- Use the Solution Explorer (right side) to browse and open files.
- Right-click the project or solution to build (`Build Solution`).
- Press F5 to run/debug the app.

---

## 🎯 1. Gather What You Need

Before you start coding, collect this info for your game:

- **Game executable name:**  
  This is the process name you see in Task Manager (e.g., `GoW.exe` for God of War 2018).

- **Memory pointers/offsets:**  
  These are the addresses and pointer chains you’ll use to read stats from the game.  
  *(If you’ve made an autosplitter or used Cheat Engine, you already know how to get these.)*
    - you will need something for loading screens, main menu detection. Since the tracker saves at those points in game via those two

- **Route file data:**  
  A list of all the items/collectibles you want to track, one per line.  
  *(You’ll create this as a `.tsv` file later in the guide.)*

💡 **Tip:**  
Use Cheat Engine or your favorite memory scanner to find the addresses for each collectible/stat you want to track. Write down the pointer chains and offsets.

---

## 🛠️ 2. Create the Game Stats Class

**Goal:**  
Add a new C# file that will read your game’s memory and provide stats to the rest of the program.

**Where:**  
File: `Route Tracker\GoW2018GameStats.cs`

---

### What is a C# class?

A class in C# is like a blueprint for an object. Here, you’ll make a class called `GoW2018GameStats` that knows how to read your game’s memory.

---

### How to add a new class in Visual Studio

1. In **Solution Explorer**, right-click the `Route Tracker` folder.  
2. Choose **Add > Class...**  
3. Name it `GoW2018GameStats.cs` and click **Add**.

---

### Example: GoW2018GameStats.cs

Below is a template. Replace the example offsets with your real pointer chains.
```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Route_Tracker
{
    // The 'unsafe' keyword allows direct memory access, which is needed for pointer reading.
    public unsafe class GoW2018GameStats : GameStatsBase
    {
        // Define your pointer chains here. Each int[] is a list of offsets for a collectible type.
        // Replace these with your actual offsets from Cheat Engine.
        private readonly int[] ravenOffsets = { 0x123, 0x456, 0x789 };
        private readonly int[] chestOffsets = { 0xABC, 0xDEF, 0x101 };
        private readonly int[] artifactOffsets = { 0x222, 0x333, 0x444 };

        // This is the base address for your pointers. Adjust the offset as needed for your game.
        private readonly nint collectiblesBaseAddress;

        // This is the constructor. It runs when the class is created.
        // 'processHandle' and 'baseAddress' are provided by the tracker.
        public GoW2018GameStats(IntPtr processHandle, IntPtr baseAddress)
            : base(processHandle, baseAddress) // Calls the parent class constructor
        {
            // Calculate the base address for your pointers.
            this.collectiblesBaseAddress = (nint)baseAddress + 0x1000; // Example offset
            Debug.WriteLine("GoW2018GameStats initialized");
        }

        // This method is required. It returns a dictionary of stats for the tracker to use.
        public override Dictionary<string, object> GetStatsAsDictionary()
        {
            // ReadWithCache<T> is a helper that reads memory and caches the result for performance.
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

        // This method tells the tracker if the game is loading or at the main menu.
        // You should update this to use your own pointers for loading/menu detection.
        public override (bool IsLoading, bool IsMainMenu) GetGameStatus()
        {
            // TODO: Replace with your own logic using memory pointers.
            return (false, false);
        }
    }
}
````

---

## 🔑 Key C# Concepts

- **class**: Defines a new type or object.  
- **public**: Makes the class or method accessible from other code.  
- **unsafe**: Allows pointer operations (needed for memory reading).  
- **readonly**: Means the variable can only be set in the constructor.  
- **constructor**: The method with the same name as the class, called when you create the object.  
- **override**: Means you’re replacing a method from the parent class (`GameStatsBase`).  
- **Dictionary<string, object>**: A key-value map, used to return stats.  

---

## ✅ What to Do Next

- Replace the example offsets with your real pointer chains for each collectible/stat.  
- If you want to track more stats, add more arrays and entries in the dictionary.  
- For loading/main menu detection, use the same approach: define the pointer chain, read the value, and return true/false as needed.  

**💡 Tip:**  
You can use `Debug.WriteLine("message")` to print debug info to the Output window in Visual Studio while testing.

---

## 🔗 3. Update GameConnectionManager

**Goal:**  
Tell the program how to recognize, connect to, and start your new game, and which stats class to use.

**Where:**  
File: `Route Tracker\GameConnectionManager.cs`

---

### ❓ What is GameConnectionManager?

This C# class handles:

- Detecting if your game is running  
- Starting your game if needed  
- Creating the correct stats reader for your game  

You’ll update or add to three methods.

---

### 3.1 Update the `DetectRunningGame()` Method

This method checks all running processes and returns the friendly name of any supported game it finds.

**C# Concepts:**

- **Dictionary**: A key-value map. Here, it maps process names to game names.  
- **foreach**: Loops through each item in a collection.  
- **try/catch**: Handles errors so the program doesn’t crash if it can’t access a process.  

**What to do:**  
Add your game’s process name and friendly name to the dictionary.
```csharp
public string DetectRunningGame()
{
    // Maps process names to friendly game names
    Dictionary<string, string> gameProcessMap = new()
    {
        { "AC4BFSP", "Assassin's Creed 4" },
        { "GoW", "God of War 2018" }
        // Add more games here as needed
    };

    // Loop through all running processes
    foreach (var process in Process.GetProcesses())
    {
        try
        {
            // Check if the process name matches any supported game
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
            // Skip processes we can't access
            continue;
        }
    }

    return string.Empty; // No matching game found
}
```

---

### 3.2 Update the `InitializeGameStats()` Method

This method creates the correct stats class for the selected game.

**C# Concepts:**

- **switch expression**: Picks which class to create based on the game name.  
- **new**: Creates a new object.  

**What to do:**  
Add a line for your new game, using your new stats class.
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

        // Subscribe to stats updates
        gameStats.StatsUpdated += OnGameStatsUpdated;
        gameStats.StartUpdating();
    }
}
```

---

### 3.3 Update the `ConnectToGameAsync()` Method

This method sets the process name, starts the game if needed, and connects to it.

**C# Concepts:**

- **async/await**: Lets the program wait for something (like a process starting) without freezing the UI.  
- **Task<bool>**: A method that runs asynchronously and returns true/false.  

**What to do:**  
Add your game’s process name and make sure it matches your executable.
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
---

## ✅ **What to Do Next**

- Make sure your game’s process name and friendly name are added in all three places above.  
- The process name (e.g., `GoW.exe`) must match exactly what you see in Task Manager.  
- The friendly name (e.g., *God of War 2018*) is what appears in the UI.  

**💡 Tip:**  
If you add more games in the future, just repeat this pattern.

---

## ⚙️ **4. Update SettingsManager**

**Goal:**  
Let the program remember where your game is installed, so it can launch and connect to it.

**Where:**  
File: Route Tracker\SettingsManager.cs

---

### **❓ What is SettingsManager?**  
This C# class manages where your games are installed (the folder with the game’s `.exe`).  
You’ll update three methods so the tracker can store and use your new game’s directory.

---

### 4.1 Update the GetGameDirectory() Method

This method returns the saved folder path for each game.

**C# Concepts:**  
- `if/else`: Checks which game is selected and returns the right directory.  
- `Settings.Default`: Stores user settings (like game paths) between runs.

**What to do:**  
Add a line for your new game.

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

---

### 4.2 Update the SaveDirectory() Method

This method saves the selected folder path for each game.

**C# Concepts:**  
- `Settings.Default.Save()`: Writes the changes to disk.  
- `BackupSettings()`: (If present) makes a backup of your settings.

**What to do:**  
Add a line for your new game.

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

---

### 4.3 Update the GetGamesWithDirectoriesSet() Method

This method returns a list of games that have their folder set.

**C# Concepts:**  
- `List<string>`: A list of strings.  
- `foreach`: Loops through each supported game.

**What to do:**  
Add your new game to the list.

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
---

### ✅ What to Do Next 

- Make sure your new game is included in all three methods above.  
- The setting name (e.g., `Gow2018Directory`) must match what you use in your project’s settings (see next step).  
#### **💡 Tip:  **
You can edit your settings in Visual Studio:  
Right-click the project > Properties > Settings tab.  

---

## 🖥️ 5. Update UI Elements  
**Goal:**
Make sure your new game appears in the dropdown menus and directory pickers in the app’s UI.  
Where:  
- Route Tracker\ConnectionWindow.cs (for connecting to a game)  
- Route Tracker\GameDirectoryForm.cs (for setting the game’s install folder)  

---

### 5.1 Update ConnectionWindow

This window lets you pick which game to connect to.  

**What to do:** 
Add your new game to the dropdown list.  

**C# Concepts:**  
- **ComboBox:** A dropdown menu control.  
- **Items.AddRange:** Adds multiple items to the dropdown.  
- **this.Controls.Add:** Adds a UI element to the window.  

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
    // Add your new game here:
    gameDropdown.Items.AddRange(["", "Assassin's Creed 4", "God of War 2018"]);
    gameDropdown.SelectedIndex = 0;
    this.Controls.Add(gameDropdown);

    // ... rest of the method remains the same
}
```

---

**💡 Tip:**  
If you add more games, just add them to the `AddRange` list.

---

## 5.2 Update GameDirectoryForm

This window lets you set the install folder for each game.

**What to do:**  
Add your new game to the dropdown and update the logic for loading/saving the directory.

**C# Concepts:**  
- **SelectedItem:** The currently selected value in the dropdown.  
- **Settings.Default:** Where the folder path is stored.

**Example for dropdown and directory loading:**

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

---

**Example for saving the directory:**
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

---

✅ **What to Do Next**  
- Make sure your new game is in the dropdowns in both files.  
- Make sure the directory logic uses the correct setting name (e.g., `Gow2018Directory`).

💡 **Tip:**  
You can test the UI by running the app (press F5 in Visual Studio) and opening the connection or directory windows.

---

🧩 **6. Add Settings Property**  
**Goal:**  
Add a new property to the app’s settings so it can remember the install folder for your new game.  
**Where:**  
- `Settings.settings` (Visual Studio’s settings designer)  
- Or, if editing code directly: `Settings.Designer.cs`

---

**What are application settings?**  
C# WinForms apps can store user settings (like folder paths) that are saved between runs.  
You’ll add a new setting for your game’s directory.

---

**How to add a new setting in Visual Studio**  
1. In Solution Explorer, right-click your project and choose **Properties**.  
2. Go to the **Settings** tab.  
3. Add a new row:  
   - **Name:** `Gow2018Directory`  
   - **Type:** `string`  
   - **Scope:** `User`  
   - **Value:** (leave empty)  
4. Save and close the settings window.

💡 **Tip:**  
If you don’t see a **Settings** tab, right-click the project > **Add** > **New Item** > **Settings File**.

---

**How to add the property in code (if needed)**  
If you prefer, you can add the property directly in `Settings.Designer.cs`:
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

---

**C# Concepts:**  
- `public string PropertyName { get; set; }`: This is a property, like a variable with built-in get/set methods.  
- `Settings.Default.PropertyName`: How you access this value in your code.

---

✅ **What to Do Next**  
- Make sure the property name matches what you used in your code (e.g., `Gow2018Directory`).  
- You can now use `Settings.Default.Gow2018Directory` anywhere in your project to get/set the folder path.

💡 **Tip:**  
You only need to do this once per new game. If you add more games, repeat this step for each one.

---

📄 **7. Create a Route File**  
**Goal:**  
Make a file that lists all the collectibles or items you want to track in your game. The tracker uses this file to know what to display and mark as complete.  

**Where:**  
- Folder: `Routes/`  
- File name: `God of War 2018 100 % Route - Main Route.tsv` (or similar)  

---

**What is a route file?**  
A route file is a plain text file with one collectible or item per line.  
It uses the TSV (Tab-Separated Values) format, which means each column is separated by a tab (not spaces or commas).  

---

**How to create a route file**  
1. Open Notepad, VS Code, or any text editor.
    - probably better to use excel or googlesheets
2. Save the file as `God of War 2018 100 % Route - Main Route.tsv` in the `Routes` folder of your project.
    - if in googlesheets or excel, export as TSV (tab-separated values)
3. Each line should have these columns, separated by tabs:

| Name/Hint        | Type       | Number | Coordinates (Optional)    |  
|------------------|------------|--------|---------------------------|  
| Raven #1         | Raven      | 1      | Midgard - Wildwoods       |  
| Nornir Chest #1  | Chest      | 1      | Midgard - River Pass      |  
| Artifact #1      | Artifact   | 1      | Midgard - Lookout Tower   |  

---

**Example file content:**  

```tsv
Raven #1	Raven	1	Midgard - Wildwoods
Nornir Chest #1	Chest	1	Midgard - River Pass
Artifact #1	Artifact	1	Midgard - Lookout Tower
Legendary Chest #1	Chest	2	Midgard - Witch's Cave
Raven #2	Raven	2	Midgard - The River Pass
````
---

**What do the columns mean?**  
- **Name/Hint:**  
  What the player is looking for (shown in the UI). Example: Raven #1  
  give like a brief description of where the item is
- **Type:**  
  The type of collectible. This must match the type names you use in your code (e.g., Raven, Chest, Artifact).

  **Advanced tip: Generating the Number column automatically in Google Sheets**  
  You can use formulas to count the occurrence of each type, so you don’t have to number them manually.

  - Basic formula to count how many of each type appear so far:  
    ```excel
    =COUNTIF($B$2:B2, B2)
    ```
    - Here, **B2** is the first data row in the Type column (row 1 is header).
  
  - If you want to count by type *and* by location (e.g., "Raven 1 Home", "Raven 2 Wildwoods"), try this:  
    ```excel
    =IFS(
      OR(B2="Raven", B2="Nornir", B2="Lore", B2="Story", B2="Upgrades", B2="Valyries"),
      COUNTIFS($B$2:B2, B2, $A$2:A2, "*" & TRIM(RIGHT(SUBSTITUTE(A2, " ", REPT(" ", 100)), 100))),
      B2<>"",
      COUNTIF($B$2:B2, B2),
      TRUE,
      ""
    )
    ```
    - Replace the list inside `OR()` with your relevant types.
    - Column **A** holds the Name/Hint values.

  - **Pro tip:** Put location hints *before* the type name in column A to help this formula work correctly.
  **Tip:** These are matched in the `CheckCompletion` method’s switch statement.  
- **Number:**  
  A unique number for each collectible of that type (used for progress tracking). Start at 1 and count up.  
- **Coordinates (Optional):**  
  In-game location for reference.  
  *Note: Mainly useful for games like AC4 or Uncharted that show coordinates on the map. For other games, location data may not be available or necessary.*

---

**Tips for making your route file**  
- Use the Tab key to separate columns (not spaces).  
- Make sure the Type column matches exactly what your code expects (case-insensitive).  
- You can add as many lines as you want—one for each collectible or item.  
- You can open the file in Excel or Google Sheets (import as TSV) to edit it more easily.  

---

✅ **What to Do Next**  
- Create your `.tsv` file and fill it with all the collectibles/items you want to track.  
- Save it in the `Routes` folder.  
- Make sure the file is tab-separated and the columns are in the correct order.  

💡 **Tip:**  
If you want to add more columns (like prerequisites), check the advanced features section later in the guide.  

---

## 🧭 8. Update `RouteManager` (If Needed)

**Goal:**  
Make sure the tracker knows how to check if each collectible in your route file is complete, using your game’s stats.

**Where:**  
File: `Route Tracker\RouteManager.cs`  
Look for the `CheckCompletion` method.

---

### 📌 What does `CheckCompletion` do?

This method matches each line in your route file (by the **Type** column) to the correct stat in your code.  
If you add new collectible types, you need to add them here so the tracker knows how to check for completion.

---

### 🛠️ How to update `CheckCompletion`

**C# Concepts:**

- `switch expression`: Picks the right logic based on the collectible type.
- `stats.GetValue<int>("StatName", 0)`: Gets the current value for a stat (like `"Ravens"` or `"Chests"`).
- `entry.Condition`: The number from your route file’s **Number** column.

**Example:**
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

---

### 🧩 What to Do

- For each new **collectible type** in your route file, add a line to the `switch` expression.
- The **left side** (e.g., `"Raven"` or `"raven"`) matches the **Type** column in your route file.
- The **right side** (e.g., `stats.GetValue<int>("Ravens", 0)`) matches the **stat name** from your `GetStatsAsDictionary()` method in your game stats class.
- If you add more stats to your game, repeat this same pattern.

---

### ✅ What to Do Next

- Make sure **every collectible type** in your route file is handled in the `switch` expression.
- The **Type** column in your route file must match (case-insensitive) the cases in the `switch`.
- If you want to add more advanced logic (like prerequisites), you can expand this method.

> 💡 **Tip:**  
> If you add a new collectible type later, just add another `case` to the `switch`.

---

### 📊 9. Update Stats Display (If Needed)

**Goal:**  
Show your new game’s stats (like Ravens, Artifacts, Chests) in the stats window of the app.

**Where:**  
File: `Route Tracker\RouteHelpers.cs`  
Look for the `BuildStatsText` method.

---

### 🔍 What does `BuildStatsText` do?

This method takes the **stats dictionary** from your game stats class and builds a string to display in the UI.  
If you want your new stats to show up, you need to **add them here**.

---

### 🛠️ How to update `BuildStatsText`

**C# Concepts:**
- `Dictionary.ContainsKey`: Checks if a stat is present.
- `GetValueOrDefault`: Gets a stat’s value, or a default if it’s missing.
- **String interpolation**: Builds a string with variable values.

> 📌 **Note:**  
> You'll typically append new lines to the final string using `\n` to display multiple stats in a readable format.


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

### 🧾 What to Do

- For each stat you want to show (e.g., **Ravens**, **Artifacts**, **Chests**), **add a line in the God of War section**.
- The **key** (e.g., `"Ravens"`) must match exactly what you return in your `GetStatsAsDictionary()` method in your game stats class.
- You can add **additional lines** for any other stats you want to display.

---

### ✅ What to Do Next

- ✅ Make sure **all the stats** you want to see in the UI are included in this method.
- ▶️ Run the app and check the **stats window** to verify your new stats appear.

💡 **Tip:**  
If you add more stats to your game later, just **add more lines** here.

---

## 🧪 10. Testing Your Implementation

**Goal:**  
Make sure everything works: the tracker connects to your game, reads stats, loads your route file, and updates progress as you play.

---

### 🧱 Step-by-Step Testing

1. **Build and run the application**  
   • In Visual Studio, press `F5` or click **Build > Build Solution**, then **Debug > Start Debugging**.

2. **Open Settings > Game Directory**  
   • Set the install folder for **"God of War 2018"** (where `GoW.exe` is located).

3. **Start God of War 2018**  
   • Launch the game and load a save file.

4. **Connect to the game**  
   • In Route Tracker, click **"Connect to Game"** and select **"God of War 2018"** in the `ConnectionWindow`.

5. **Check the Game Stats window**  
   • Ensure stats (Ravens, Artifacts, Chests, etc.) are updating as you collect items.

6. **Verify route file loading**  
   • The route entries from your `.tsv` file should appear in the tracker.

7. **Test progress tracking**  
   • As you collect items in-game, the tracker should mark them as complete.

8. **Check autosave and backups**  
   • Progress should be saved automatically, and backup files should appear in your save directory.

9. **Try enhanced UI features**  
   • Test filtering, search, and different layout modes if available.

---

### ✅ Debugging Checklist

- [ ] Game executable name matches exactly (`GoW.exe`)
- [ ] Memory addresses are correct for your game version
- [ ] Route file is properly formatted (tab-separated, not spaces)
- [ ] Collectible types in route file match the switch statement in `CheckCompletion`
- [ ] Settings directory is set correctly
- [ ] `ConnectionWindow` shows your game in the dropdown
- [ ] Autosave system creates backup files correctly

---

### 🧯 If Something Doesn’t Work

**Game not detected:**  
Double-check the process name in Task Manager and your code.

**Stats not updating:**  
Make sure your memory addresses and pointer chains are correct.  
Use `Debug.WriteLine()` to log values.

**Route file not loading:**  
Check for **tabs (not spaces)** and correct column order.  
File must be in the **Routes** folder.

**UI issues:**  
Make sure your game is added to **all dropdowns and settings**.

---

💡 **Tip:**  
You can use **breakpoints** and the **Output window** in Visual Studio to debug issues.  
If you get stuck, check the **troubleshooting section** at the end of this guide.

---

## 🏗️ 11. Understanding the New Architecture

**Goal:**  
Get a high-level view of how the Route Tracker codebase is organized, so you know where to look when you want to add features or troubleshoot.

---

### 🧰 Key Helper Classes

- **`RouteHelpers.cs`**  
  Handles route data management, filtering, connecting to games, and window management.

- **`MainFormHelpers.cs`**  
  Manages UI creation, context menus, and hotkey processing.

- **`SettingsMenuManager.cs`**  
  Handles the settings menu creation and management.

- **`LayoutManager.cs`**  
  Controls UI layout for different modes (Normal, Compact, Mini, Overlay).

---

### 🚀 Enhanced Features

- **Memory Caching**  
  `GameStatsBase` now caches memory reads for better performance.

- **Adaptive Timers**  
  Update frequency changes based on player activity.

- **Cycling Backups**  
  Progress is saved with numbered backups (1–10) that cycle automatically.

- **Settings Backup**  
  Settings are backed up to AppData and can be restored.

- **Enhanced UI**  
  Multiple layout modes, improved connection window, and better filtering/search.

---

### 🗂️ Key Files to Update When Adding a Game

1. **`GameStatsBase`**  
   Your game stats class (inherits enhanced features).

2. **`GameConnectionManager`**  
   Game detection and connection logic.

3. **`SettingsManager`**  
   Game directory management.

4. **`ConnectionWindow`**  
   Game selection and connection UI.

5. **`GameDirectoryForm`**  
   Directory selection UI.

6. **`RouteManager`**  
   Route completion logic (if needed).

7. **`RouteHelpers`**  
   Stats display (if desired).

---

### 📌 Why This Matters

- The codebase is now **modular**, so you only need to update a few files to add a new game.
- **Helper classes** keep the code organized and easier to maintain.
- **Enhanced features** (like autosave, backup, and adaptive timers) work automatically for all games.

---

💡 **Tip:**  
If you want to add **advanced features** (like new UI modes or backup logic), look in the relevant helper class.

---

## ✅ 12. Complete Implementation Checklist

Before you consider your new game fully integrated, make sure you have:

- [ ] **Created `GoW2018GameStats.cs`** with correct memory reading logic.
- [ ] **Added all pointer offsets** for collectibles/stats.
- [ ] **Updated** `GameConnectionManager.DetectRunningGame()`.
- [ ] **Updated** `GameConnectionManager.InitializeGameStats()`.
- [ ] **Updated** `GameConnectionManager.ConnectToGameAsync()`.
- [ ] **Updated** `SettingsManager.GetGameDirectory()`.
- [ ] **Updated** `SettingsManager.SaveDirectory()`.
- [ ] **Updated** `SettingsManager.GetGamesWithDirectoriesSet()`.
- [ ] **Added** your game to the **ConnectionWindow** dropdown.
- [ ] **Added** your game to the **GameDirectoryForm** dropdown.
- [ ] **Added** the `Gow2018Directory` setting property.
- [ ] **Created** your route `.tsv` file with the correct format.
- [ ] **Updated** `RouteManager.CheckCompletion()` if needed.
- [ ] **Updated** `RouteHelpers.BuildStatsText()` if desired.
- [ ] **Tested** the complete workflow with enhanced features.
- [ ] **Verified** the cycling autosave system works.
- [ ] **Tested** connection window functionality.
- [ ] **Verified** the settings backup system works.

---

## 🗂️ 13. Route File Format Reference

**Key points:**

- Use **tabs** (not spaces) to separate columns.
- The **Type** column must **exactly match** your switch statement cases in `CheckCompletion`.
- **Numbers** should be sequential for each collectible type.
- **Coordinates** are optional but helpful for players.

**Example:**
```tsv
Raven #1	Raven	1	Midgard - Wildwoods
Nornir Chest #1	Chest	1	Midgard - River Pass
Artifact #1	Artifact	1	Midgard - Lookout Tower
```

---

## 🚀 14. Advanced Features (Optional)

The enhanced architecture supports:

### • Memory Optimization
- `ReadWithCache()` for efficient memory access  
- Automatic cache invalidation and adaptive update rates  

### • Enhanced Backup System
- Cycling progress backups (1–10) with automatic rotation  
- Settings backup to AppData with restore/rollback  

### • Improved UI
- Multiple layout modes (**Normal**, **Compact**, **Mini**, **Overlay**)  
- Enhanced filtering and search  
- Dedicated connection window and right-click context menus  

### • Advanced Route Features
- Prerequisite system for collectibles  
- Better completion tracking  
- Improved file searching with fallback locations  
- Support for game state transitions (loading, menu, gameplay)  

> You can explore these features as you get more comfortable with the codebase.

---

## 🛑 15. Troubleshooting Common Issues

### **Game Not Detected**
- [ ] Check the **process name** in Task Manager and your code  
- [ ] Ensure your game is listed in the **ConnectionWindow dropdown** and `DetectRunningGame()`  

### **Stats Not Updating**
- [ ] Double-check **memory addresses and pointer chains**  
- [ ] Use `Debug.WriteLine()` to log values  
- [ ] Confirm your stats class **inherits from `GameStatsBase`**  

### **Route File Not Loading**
- [ ] Ensure the file is **tab-separated** and in the correct folder  
- [ ] The **Type column** must match your **switch statement**  
- [ ] Test with the **enhanced file searching** system  

### **Connection Issues**
- [ ] Use the **new ConnectionWindow**  
- [ ] Set the game directory in **Settings**  
- [ ] Run Route Tracker as **administrator** if needed  

### **UI Issues**
- [ ] Make sure all **helper classes are updated**  
- [ ] Check that your game appears in **all dropdowns**  
- [ ] Test different **layout modes**

---

## 🎉 16. Congratulations!

You’ve successfully added your new game to **Route Tracker** using the enhanced architecture!  
You now benefit from:

- [x] Automatic **memory caching** for better performance  
- [x] **Cycling backup system** to protect your progress  
- [x] **Enhanced UI** with multiple layout modes  
- [x] Better **connection management** and **settings backup**  
- [x] Improved **filtering and search**  

> For questions or issues:  
> Check **code comments**, the **troubleshooting section**, or open an issue on the project’s **GitHub repository**.

---
