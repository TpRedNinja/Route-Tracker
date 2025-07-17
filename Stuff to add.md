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

---

- [x] 2. new sorting options
    *ideas for new sorting options*
    - by type (more for like applying to the sorting options below and the current way of sorting. so instead of by entry id u do by type)
    - remove completed ie hide completed entries but still keep a list of completed in a file
    - put completed entires at the bottom (so instead of finding the first uncompleted entry it finds the last uncompleted and put completed down their still scrolls to first)
    - save last used sorting option
    - add to backup folder like with settings 

---

---

# features to add
## 📝 Future Features TODO List

---
## v0.9-beta features list

- [ ] 3. exporting current progress to TSV/CSV for sharing
    - *3.1 through 3.3 below are all prerequisites for 3 (exporting).*
    - [ ] 3.1 multiple type filtering 
        - *how it will work*
            - each type should be like a checklist item thing
            - ie when u click on it a check mark appears 
            - if all types has a check mark then all the other types also get check marks
            - but if u uncheck one of the types then the all checked gets the checkmark removed but all other types still have their check marks
            - when the route isnt loaded all types option should have a check
    - [ ] 3.2 search history dropdown *(remember recent searches)*
        - *how it maybe would work whatever is doable is fine*
            - meant for searching for a entry 
            - basically ig we type something click enter then it gets added to search history 
            - or something like that, although the searching is instant so idk how that would work
    - [ ] 3.3 remove specific types or entries in general (e.g., upgrades, story, chests)
        - *how it will work*
            - basically theirs a button or in the drop down 
            - theirs a option to remove all uncheked types
            - this will exclude the option all types since its tied to everything
            - to specify more, if the user unchecks upgrades and chests 
            - then clicks the button it will remove all upgrades and chests from the route
    - [ ] if all things above are done then move onto the exporting feature
        - *how it will work*
            - in settings panel a new option to export current progress 
                - underneath the import route from url option
                - also have it when right clicking the route grid put it under neath everything else
                - have a seperator line above it
            - opens a window with options to choose between tsv or csv recommend tsv
            - option to choose the location to save the file
            - option to include completed entries or not
            - option to include skipped entries or not
            - option to include only filtered entries or all entries
            - button to start the export
            - shows a window with a progress bar while exporting
            - once done closes the window and shows a message box of download complete
            - hotkey/shortcut to quick open the window fully customizable (default ctrl + e)(or similar key combo)

---

I want you to add a new feature its called multi type filtering.
I currently have a filtering system that allows me to filter by one type at a time.
I want u to make it so i can filter by multiple types at once.
Currently when i filter by one type it shows the current type in the box
when i have multiply types selected i want u to show instead "Multiple Types".
Currently by default "All Types" is selected when no route is loaded
I want to keep it that way.
Their should be some visual indicator that multiple types are selected in the drop down
it can be a check mark or something else.
If "All Types" is selected then all other types should also be selected.
When no types are selected it should on the display show nothing and in the box say "None".
If the user unselects one of the types when "All Types" is selected
then "All Types" should be unselected but all other types that werent unselected should remain selected.
Ideally u shouldnt need to change much of anything in the current filtering system
But if u do its fine as long as it works and *DOES NOT BREAK ANY OTHER FEATURES*
U should make optimized code that isnt 100's of lines long
it shouldnt give me errors and it shouldnt give me messages saying whatever can be simplified
When generating me the code *GENERATE EVERYTHING I NEED* I WILL NOT STAND FOR TLDR COMMENTS
What i mean by tldr is comments like "// rest of code goes here" or "// current implementation"
I want the full code for everything i need to add this feature DO NOT SKIP ANYTHING or say do it yourself
if you create new files be sure to specify what type of file it is
DO NOT CHANGING ANYTHING THAT DOESNT NEED TO BE CHANGED KEY WORD NEED

---

## features for v1.0
- [ ] 4. logging system
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

- [ ] 5. bulk route download(maybe)
    *concerns*
        - only way to do it is to allow the user to submit multiple links ie several text boxes or a text area to paste multiple links

---

- [ ] 6. launcher or tutorial launcher(last thing to add)
    - update the md files while doing this features
    - last feature to add
    - should include a thing for every feature in the app along with images
    - should be indepth tutorial with images for each feature
    - should be able to be disabled in settings once main app is opened

---

---

# stuff to update TODO list
- [ ] ReadMe.md
- [ ] adding a new game.md
- [ ] planned stuff.md