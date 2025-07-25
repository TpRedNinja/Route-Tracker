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

- [x] 1. new sorting options
    *ideas for new sorting options*
    - by type (more for like applying to the sorting options below and the current way of sorting. so instead of by entry id u do by type)
    - remove completed ie hide completed entries but still keep a list of completed in a file
    - put completed entires at the bottom (so instead of finding the first uncompleted entry it finds the last uncompleted and put completed down their still scrolls to first)
    - save last used sorting option
    - add to backup folder like with settings 

---
- [X] 2. Updating the filtering and search bar
    - [x] 2.1 multiple type filtering 
        - *how it will work*
            - each type should be like a checklist item thing
            - ie when u click on it a check mark appears 
            - if all types has a check mark then all the other types also get check marks
            - but if u uncheck one of the types then the all checked gets the checkmark removed but all other types still have their check marks
            - when the route isnt loaded all types option should have a check
    - [x] 2.2 search history dropdown *(remember recent searches)*
        - *how it maybe would work whatever is doable is fine*
            - meant for searching for a entry 
            - basically ig we type something click enter then it gets added to search history 
            - or something like that, although the searching is instant so idk how that would work
---
- [x] 3. logging system
    - app passowrd for google account: cjut ymzr citu xvij
    - Log errors and important events with timestamps (including dates)
    - Save logs locally in a file (append, don’t overwrite)
    - On application close (with user consent), email log file automatically to developer
    - Use dedicated Gmail account (e.g., TpRouteTrackerApp@gmail.com) for sending emails
    - Authenticate email sending via Gmail SMTP with App Password (16-char, generated)
    - Allow user to add optional details/comment before sending log
    - Detect developer’s own device (by device name or unique ID) to skip sending emails during debugging
    - Clear log file after successful email sending (optional fallback if user consents)
    - Notify user about privacy, consent, and what data is sent (transparency)
    - Provide manual option to send log anytime from the UI (not just on exit)
    - only create new file if version of application is different and add a -------- seperator if the date isnt the same as last time it was logged
    - easy to refrence for replacing all current error handlers and for in general program use when its running error handling
    - developer email address to send to topredninja@gmail.com or hjdomangue@outlook.com if first email fails
---

---

# features to add
## 📝 Future Features TODO List

---

## features to add eventually

---

- [ ]. launcher or tutorial launcher(last thing to add)
    - update the md files while doing this features
    - last feature to add
    - should include a thing for every feature in the app along with images
    - should be indepth tutorial with images for each feature
    - should be able to be disabled in settings once main app is opened

---

# Instructions for Sonnet Copilot

## What I Want



## How to Generate the Code for the Feature

see `#Copilot rules.md` & `#Generating code rules.md`

---

---

# stuff to update TODO list
- [ ] ReadMe.md
- [x] adding a new game.md
- [x] planned stuff.md

---

# 📋 Summary Priority List
- [ ] HIGH PRIORITY: Implement button/control factory methods (saves 100+ lines)
- [x] HIGH PRIORITY: Consolidate hotkey processing (reduces MainForm by 60+ lines)
- [x] MEDIUM PRIORITY: Fix timer management in GameStatsBase (prevents memory leaks)
- [ ] MEDIUM PRIORITY: Create form initialization extension methods
- [ ] MEDIUM PRIORITY: Implement consistent error handling pattern
- [x] LOW PRIORITY: Optimize layout switching and font caching
- [x] LOW PRIORITY: Consolidate scroll logic into extension methods