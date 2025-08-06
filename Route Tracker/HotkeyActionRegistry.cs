using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // Central registry for all hotkey actions - eliminates repetitive if chains
    // Maps hotkey names to their corresponding actions
    public static class HotkeyActionRegistry
    {
        // ==========MY NOTES==============
        // Dictionary mapping action names to their corresponding methods
        // This replaces 25+ if statements with a simple dictionary lookup
        private static readonly Dictionary<string, Action<MainForm>> Actions = new()
        {
            ["Load"] = (form) => MainFormHelpers.LoadRouteFile(form),
            ["Save"] = (form) => form.routeManager?.SaveProgress(form),
            ["LoadProgress"] = (form) => HandleLoadProgress(form), // Enhanced to support autosave
            ["ResetProgress"] = (form) => MainFormHelpers.ResetProgress(form, form.GetRouteManager()),
            ["Refresh"] = (form) => form.LoadRouteDataPublicManager(),
            ["Help"] = (form) => {
                using var wizard = new HelpWizard(new HotkeysSettingsForm(form.settingsManager));
                wizard.ShowDialog(form);
            },
            ["FilterClear"] = (form) => RouteHelpers.ClearFilters(form),
            ["Connect"] = (form) => {
                using var connectionWindow = new ConnectionWindow(form.gameConnectionManager, form.settingsManager);
                connectionWindow.ShowDialog(form);
            },
            ["GameStats"] = (form) => RouteHelpers.ShowStatsWindow(form, form.gameConnectionManager),
            ["RouteStats"] = (form) => RouteHelpers.ShowCompletionStatsWindow(form, form.GetRouteManager()),
            ["LayoutUp"] = (form) => form.CycleLayout(true),
            ["LayoutDown"] = (form) => form.CycleLayout(false),
            ["BackupFolder"] = (form) => form.settingsManager.OpenBackupFolder(),
            ["BackupNow"] = (form) => {
                form.settingsManager.BackupSettings();
                MessageBox.Show("Settings backed up successfully!", "Backup Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            },
            ["Restore"] = (form) => HandleRestore(form),
            ["SetFolder"] = (form) => form.settingsManager.OpenSettingsFolder(),
            ["AutoTog"] = (form) => HandleAutoToggle(form),
            ["TopTog"] = (form) => {
                form.TopMost = !form.TopMost;
                form.settingsManager.SaveAlwaysOnTop(form.TopMost);
            },
            ["AdvTog"] = (form) => HandleAdvancedToggle(form),
            ["GlobalTog"] = (form) => HandleGlobalToggle(form),
            ["SortingUp"] = (form) => form.CycleSorting(true),
            ["SortingDown"] = (form) => form.CycleSorting(false),
            ["GameDirect"] = (form) => form.OpenGameDirectory(),
            ["LoadAutoSave"] = (form) => MainFormHelpers.LoadAutoSave(form) // New autosave action
        };

        // ==========MY NOTES==============
        // Main entry point - replaces the entire chain of if statements
        // Returns true if hotkey was handled, false otherwise
        public static bool HandleHotkey(MainForm mainForm, Keys keyData)
        {
            var shortcuts = mainForm.settingsManager.GetShortcuts();

            // Create mapping of Keys to action names
            var hotkeyMap = new Dictionary<Keys, string>
            {
                [shortcuts.Load] = "Load",
                [shortcuts.Save] = "Save",
                [shortcuts.LoadProgress] = "LoadProgress",
                [shortcuts.ResetProgress] = "ResetProgress",
                [shortcuts.Refresh] = "Refresh",
                [shortcuts.Help] = "Help",
                [shortcuts.FilterClear] = "FilterClear",
                [shortcuts.Connect] = "Connect",
                [shortcuts.GameStats] = "GameStats",
                [shortcuts.RouteStats] = "RouteStats",
                [shortcuts.LayoutUp] = "LayoutUp",
                [shortcuts.LayoutDown] = "LayoutDown",
                [shortcuts.BackupFolder] = "BackupFolder",
                [shortcuts.BackupNow] = "BackupNow",
                [shortcuts.Restore] = "Restore",
                [shortcuts.SetFolder] = "SetFolder",
                [shortcuts.AutoTog] = "AutoTog",
                [shortcuts.TopTog] = "TopTog",
                [shortcuts.AdvTog] = "AdvTog",
                [shortcuts.GlobalTog] = "GlobalTog",
                [shortcuts.SortingUp] = "SortingUp",
                [shortcuts.SortingDown] = "SortingDown",
                [shortcuts.GameDirect] = "GameDirect"
            };

            // Single lookup instead of 25+ if statements!
            if (hotkeyMap.TryGetValue(keyData, out string? actionName) &&
                Actions.TryGetValue(actionName, out var action))
            {
                try
                {
                    action(mainForm);
                    return true;
                }
                catch (Exception ex)
                {
                    LoggingSystem.LogError($"Error executing hotkey action {actionName}", ex);
                    return false;
                }
            }

            return false;
        }

        #region Helper Methods for Complex Actions
        // ==========MY NOTES==============
        // Helper methods for actions that need more complex logic

        private static void HandleLoadProgress(MainForm form)
        {
            // Show a dialog to choose between manual save and autosave
            var result = MessageBox.Show(
                "Load Progress Options:\n\n" +
                "Yes - Load from file (manual save)\n" +
                "No - Load autosave\n" +
                "Cancel - Cancel loading",
                "Load Progress",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            switch (result)
            {
                case DialogResult.Yes:
                    // Load manual save
                    MainFormHelpers.LoadProgress(form);
                    break;
                case DialogResult.No:
                    // Load autosave
                    MainFormHelpers.LoadAutoSave(form);
                    break;
                case DialogResult.Cancel:
                    // Do nothing
                    break;
            }
        }

        private static void HandleRestore(MainForm form)
        {
            var (hasBackup, backupDate, backupVersion) = form.settingsManager.GetBackupInfo();
            if (hasBackup)
            {
                var result = MessageBox.Show(
                    $"Restore settings from backup?\n\nBackup Date: {backupDate:yyyy-MM-dd HH:mm}\nVersion: {backupVersion}",
                    "Restore Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes && form.settingsManager.RestoreFromBackup())
                {
                    MessageBox.Show("Settings restored successfully!", "Restore Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("No backup found!", "No Backup",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void HandleAutoToggle(MainForm form)
        {
            if (form.enableAutoStartMenuItem != null)
            {
                form.enableAutoStartMenuItem.Checked = !form.enableAutoStartMenuItem.Checked;
                form.settingsManager.SaveSettings(Settings.Default.GameDirectory,
                    form.enableAutoStartMenuItem.Checked ? Settings.Default.AutoStart : "");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        private static void HandleAdvancedToggle(MainForm form)
        {
            var (CompleteHotkey, SkipHotkey, UndoHotkey, GlobalHotkeys, AdvancedHotkeys) =
                form.settingsManager.GetAllHotkeySettings();
            form.settingsManager.SaveHotkeySettings(UndoHotkey, GlobalHotkeys, !AdvancedHotkeys);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        private static void HandleGlobalToggle(MainForm form)
        {
            var (CompleteHotkey, SkipHotkey, UndoHotkey, GlobalHotkeys, AdvancedHotkeys) =
                form.settingsManager.GetAllHotkeySettings();
            form.settingsManager.SaveHotkeySettings(UndoHotkey, !GlobalHotkeys, AdvancedHotkeys);
            form.UpdateGlobalHotkeys();
        }
        #endregion
    }
}