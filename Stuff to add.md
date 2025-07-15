# ✅ Completed features and hotkeys and stuff
## These actions that have hotkey/shortcut support in your app.
- [x] Load Route File  (crtl + o)  
- [x] Save Progress  (crtl + s)  
- [x] Load Progress  (crtl + l)  
- [x] Complete Entry (hotkey)  
- [x] Skip Entry  (hotkey)  
- [x] Undo Entry  (hotkey)  
- [x] Clear Filters (esc)  
- [x] Show Help/Hotkey Wizard (F1)  
- [x] refresh (F5)  
- [x] Reset Progress (crtl + r)  
- [x] Connect to Game (shift + c)  
- [x] Show Game Stats Window (shift + s)  
- [x] Show Route Stats Window (shift + r)  
- [x] Open Backup Folder (crtl + b)  
- [x] Backup Settings Now (shift + b)  
- [x] Restore Settings from Backup (crtl + shift + b)  
- [x] Open Settings Folder (crtl + shift + s)
- [x] Auto-Start Game *(toggle)*  (crtl + a)
- [x] Toggle Always On Top  (crtl + t)
- [x] Toggle Advanced Hotkeys  (shift + a)
- [x] Toggle Global Hotkeys  (crtl + g)
- [x] open import route from url window (crtl + u)
- [x] Switch Layout Mode *(Normal / Compact / Mini / Overlay)*  
	- for up: alt + m  
	- for down: shift + m  

---

## 🛠️ UpdateManager Advanced Checklist

- [x] Show modal progress window during update
    - [x] Progress bar for downloading ZIP
    - [x] Progress bar for extracting ZIP
    - [x] Status label for current operation
    - [x] Allow user to select download location
    - [x] Allow user to select extraction location
- [x] "Continue" button (disabled until extraction is complete)
- [x] Checkbox "Launch new version after update" (disabled until extraction is complete)
- [x] On completion:
    - [x] If checkbox checked, close all app windows and launch new EXE
    - [x] Optionally launch as admin (if needed)
- [x] Error handling for download and extraction
- [x] Show error messages if update fails

---

### 🎹 Keyboard Shortcuts  
Let the user customize their shortcuts. *(Done)*

- ✅ Ctrl+O → Load Route  
- ✅ Ctrl+S → Save Progress  
- ✅ F5 → Refresh  
- ✅ F1 → Help  
- ✅ Esc → Clear Filters

---

### 🧽 UI Polish & Quality of Life

- ✅ Tooltips explaining what each button/feature does

---

### 📖 Documentation & Help

- ✅ Built-in help system *(F1 key or Help menu)*
- ✅ Keyboard shortcuts reference dialog *(done as part of help system)*

---

### 🛠️ other random features

- ✅ Global hotkeys
- ✅ More hotkeys such as undo
- ✅ Toggles and more shortcuts for miscellaneous features

---

- [x] 1. import routes from URLs (GitHub raw links, etc.)
    - *description:*
        - option to import route from a url to a raw github link or other platforms with similar raws
    - *how it would work:*
        - in settings panel, a new option to import route from url
        - opens a window with a text box to paste the url
        - tells the user before hand that it must be a raw link ie from github or gist or similar websites with similar types of links
        - has the option to load the route after downloading
        - allows the user to choose the name of the route
        - hotkey topen the window (ctrl + u)(or similar key combo)
        - save downloaded routes to the backup folder like with settings
        - historty of downloaded routes(maybe)

*message for sonnet*

#solution

Add a feature to my C# WinForms app (.NET 8) called "Import routes from URLs" with the following requirements:

**Requirements:**
- User can import a route file from any direct/raw URL (e.g., GitHub raw, Gist, or similar).
- Access this feature from the Settings panel as a new menu item.
- When selected, open a dialog with:
  - Textbox for the URL.
  - Textbox for the desired route file name.
  - Button to start download.
  - Checkbox: "Load route after downloading."
  - Label: explain only direct/raw links are supported.
  - Progress bar for download progress.
- After download, save the file to the backup folder (same as settings backups).
- Keep a history of all downloaded routes (for restore/re-import).
- If "Load after download" is checked, load the route immediately.
- If not, show a "Download Complete" message or screen.
- Add a customizable hotkey (default: Ctrl+U) to open the import dialog. This hotkey must be configurable in the hotkeys window.
- The restore from backup feature must support restoring downloaded routes.
- Code should be structured for easy future expansion to bulk download (separate download logic and UI).
- Handle errors: invalid URLs, download failures, file name conflicts.

**Instructions:**
- Generate all code for this feature:
  - New forms, classes, or methods as needed.
  - Full function bodies for any replacements or modifications.
  - All UI code for the dialog (code or designer, but show all relevant code).
  - Integrate with settings menu and hotkey system, including hotkey customization.
  - Integrate with restore from backup for downloaded routes.
  - Include all error handling.
- For each code block, specify:
  - File name and path (e.g., `Route Tracker\ImportRouteForm.cs`).
  - Where in the file to place the code (e.g., "add this method to MainForm.cs").
  - If a new file, specify the type (form, class, etc.).
- Do not use placeholders like "rest of code goes here." Show full code for any function or class you add or modify.
- At the end, provide a step-by-step integration guide summarizing where to add each piece and how to wire up the feature.
- Ensure the code is easy to expand for bulk download support in the future.

---

---

# features to add
## 📝 Future Features TODO List

---

- [ ] 2. new sorting options
    *ideas for new sorting options*
    - by type (more for like applying to the sorting options below and the current way of sorting. so instead of by entry id u do by type)
    - remove completed ie hide completed entries but still keep a list of completed in a file
    - put completed entires at the bottom (so instead of finding the first uncompleted entry it finds the last uncompleted and put completed down their still scrolls to first)
    - save last used sorting option
    - add to backup folder like with settings 

#solution

Add a feature to my C# WinForms app (.NET 8) called "New Sorting Options" with the following requirements:

**Requirements:**
- Add new sorting options for the route grid:
  - Option to hide completed entries (but keep a list of completed in a file. Technically already have with save progress; just ensure it works).
  - Option to put completed entries at the bottom (scroll to first uncompleted. Already have; just ensure it works).
  - Save and restore the last used sorting option.
  - Add sorting settings to the backup folder (just add it to settings; extend current implementation).
- Access sorting options from the UI:
  - Add a button in the settings panel (like the layout options) that opens a separate window for sorting options.
  - Also allow access via customizable hotkeys to swap between all sorting modes in both up and down directions (defaults: up = Alt+D, down = Shift+D). These hotkeys must be configurable in the hotkeys window.
- All sorting logic should be integrated with the route grid and update the display accordingly.
- Error handling for edge cases (e.g., empty grid, all entries completed).
- Code should be structured for easy future expansion (e.g., adding more sort modes).

**Additional Requirement:**
- Add a customizable hotkey (default: Ctrl+D) to open the game directory window. This hotkey must be configurable in the hotkeys window.

**Hotkey Persistence:**
- All new hotkeys (sorting up, sorting down, game directory) must be saved and loaded with the rest of the hotkeys/shortcuts, using the same persistence mechanism as existing hotkeys.

**Instructions:**
- Generate all code for this feature, including:
  - New forms, classes, or methods as needed.
  - Full function bodies for any replacements or modifications.
  - All UI code for the sorting options window (code or designer, but show all relevant code), and the settings panel button to open it.
  - Integrate with the settings menu and hotkey system, including hotkey customization, wiring, and persistence for all new hotkeys.
  - **Explicitly generate all code for adding the hotkeys, including registration, configuration, event handling, and saving/loading with the rest of the hotkeys. Do not just reference existing logic—show the actual code needed.**
  - Integrate sorting settings and the new hotkeys with the backup/restore system.
  - Include all error handling.
- For each code block, specify:
  - File name and path (e.g., `Route Tracker\SortingOptionsForm.cs`).
  - Where in the file to place the code (e.g., "add this method to MainForm.cs").
  - If a new file, specify the type (form, class, etc.).
- Do not use placeholders like "rest of code goes here." Show full code for any function or class you add or modify.
- At the end, provide a step-by-step integration guide summarizing where to add each piece and how to wire up the feature.
- Ensure the code is easy to expand for future sorting modes.

Thank you!
    
---

## *will add details about these later*

- [ ] 3. exporting current progress to TSV/CSV for sharing
    - [ ] 3.1 multiple type filtering *(Ctrl+Click to select multiple types)*
    - [ ] 3.2 search history dropdown *(remember recent searches)*
    - [ ] 3.3 remove specific types or entries in general (e.g., upgrades, story, chests)
    - [ ] 3.4 remove all of a entry with a certain type with one button click
    *- 3.1 - 3.4 are all prerequisites for 3 (exporting).*

---

- [ ] 4. bulk route download(maybe)
    *concerns*
        - only way to do it is to allow the user to submit multiple links ie several text boxes or a text area to paste multiple links
- [ ] 5. launcher or tutorial launcher(last thing to add)
    - update the md files while doing this features
    - last feature to add
    - should include a thing for every feature in the app along with images
    - should be indepth tutorial
    - should be able to be disabled in settings

---

---

# stuff to update TODO list
- [ ] ReadMe.md
- [ ] adding a new game.md
- [ ] planned stuff.md