## ✅ Completed  
These actions already have hotkey/shortcut support in your app.

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
- [x] Switch Layout Mode *(Normal / Compact / Mini / Overlay)*  
	- [x] for up: crtl + m  
	- [x] for down: shift + m  
- [x] Open Backup Folder (crtl + b)  
- [x] Backup Settings Now (shift + b)  
- [x] Restore Settings from Backup (crtl + shift + b)  
- [x] Open Settings Folder (crtl + shift + s)
- [x] Auto-Start Game *(toggle)*  (crtl + a)
- [x] Toggle Always On Top  (crtl + t)
- [x] Toggle Advanced Hotkeys  (shift + a)
- [x] Toggle Global Hotkeys  (crtl + g)

---

## 🔴 Need to Implement Feature First  
These require the feature to be implemented before you can add hotkey/shortcut support.

- Export Route/Progress  
- Bulk Route Download  
- Remove Entry  
- Scroll to First Incomplete Entry *(goes with sort route grid)*  
- Sort Route Grid *(toggle sort modes)*

-----

## features to add
# 📝 Future Features TODO List

---

## 📁 Import/Export Features

- [ ] Import routes from URLs (GitHub raw links, etc.)
- [ ] Export current progress to TSV/CSV for sharing *(maybe)*

---

## 🔎 Enhanced Search & Filtering

- [ ] Multiple type filtering *(Ctrl+Click to select multiple types)*
- [ ] Search history dropdown *(remember recent searches)*

---

## 🎹 Keyboard Shortcuts  
Let the user customize their shortcuts. *(Done)*

- ✅ Ctrl+O → Load Route  
- ✅ Ctrl+S → Save Progress  
- ✅ F5 → Refresh  
- ✅ F1 → Help  
- ✅ Esc → Clear Filters

---

## 🧽 UI Polish & Quality of Life

- ✅ Tooltips explaining what each button/feature does

---

## 📖 Documentation & Help

- ✅ Built-in help system *(F1 key or Help menu)*
- ✅ Keyboard shortcuts reference dialog *(done as part of help system)*
- [ ] Launcher or tutorial launcher

---

## 🛠️ More Features

- ✅ Global hotkeys
- ✅ More hotkeys such as undo
- ✅ Toggles and more shortcuts for miscellaneous features
- [ ] Other sorting options  
  *(e.g., remove completed entries or put them at the bottom of the scroll bar)*
- [ ] Allow users to remove specific types or entries in general (e.g., upgrades, story, chests)  
  - [ ] Program won’t try to track them  
  - [ ] Remove all of a type with one button click

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
