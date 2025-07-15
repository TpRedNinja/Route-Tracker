using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
using Route_Tracker.Properties;

namespace Route_Tracker
{
    public static class SettingsMenuManager
    {
        // ==========MY NOTES==============
        // Builds the Settings dropdown menu with its options
        [SupportedOSPlatform("windows6.1")]
        public static void CreateSettingsMenu(MainForm mainForm, MenuStrip menuStrip, SettingsManager settingsManager)
        {
            ToolStripMenuItem settingsMenuItem = new("Settings");

            UpdateManager.AddUpdateCheckMenuItem(settingsMenuItem);
            UpdateManager.AddDevModeMenuItem(settingsMenuItem);

            // Auto-Start Game UI
            mainForm.autoStartGameComboBox = new ToolStripComboBox
            {
                Name = "autoStartGameComboBox",
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 180
            };

            var gamesWithDirs = settingsManager.GetGamesWithDirectoriesSet();
            mainForm.autoStartGameComboBox.Items.AddRange([.. gamesWithDirs]);
            mainForm.autoStartGameComboBox.SelectedIndex = -1;

            string savedAutoStart = Settings.Default.AutoStart;
            if (!string.IsNullOrEmpty(savedAutoStart) && gamesWithDirs.Contains(savedAutoStart))
            {
                mainForm.autoStartGameComboBox.SelectedItem = savedAutoStart;
            }

            mainForm.autoStartGameComboBox.SelectedIndexChanged += (s, e) => AutoStartGameComboBox_SelectedIndexChanged(mainForm);
            settingsMenuItem.DropDownItems.Add(new ToolStripLabel("Auto-Start Game:"));
            settingsMenuItem.DropDownItems.Add(mainForm.autoStartGameComboBox);

            mainForm.enableAutoStartMenuItem = new ToolStripMenuItem("Enable Auto-Start")
            {
                CheckOnClick = true,
                Checked = !string.IsNullOrEmpty(savedAutoStart),
                Enabled = mainForm.autoStartGameComboBox.SelectedItem != null
            };
            mainForm.enableAutoStartMenuItem.CheckedChanged += (s, e) => EnableAutoStartMenuItem_CheckedChanged(mainForm);
            settingsMenuItem.DropDownItems.Add(mainForm.enableAutoStartMenuItem);

            // Other menu items
            ToolStripMenuItem gameDirectoryMenuItem = new("Game Directory");
            gameDirectoryMenuItem.Click += (s, e) => GameDirectoryMenuItem_Click(mainForm);

            ToolStripMenuItem alwaysOnTopMenuItem = new("Always On Top")
            {
                CheckOnClick = true,
                Checked = mainForm.TopMost
            };
            alwaysOnTopMenuItem.CheckedChanged += (s, e) => AlwaysOnTopMenuItem_CheckedChanged(mainForm, settingsManager, s);

            settingsMenuItem.DropDownItems.Add(gameDirectoryMenuItem);
            settingsMenuItem.DropDownItems.Add(alwaysOnTopMenuItem);

            menuStrip.Items.Add(settingsMenuItem);

            settingsMenuItem.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem hotkeysMenuItem = new("Configure Hotkeys");
            hotkeysMenuItem.Click += (s, e) => HotkeysMenuItem_Click(mainForm, settingsManager);
            settingsMenuItem.DropDownItems.Add(hotkeysMenuItem);

            ToolStripMenuItem enableHotkeysMenuItem = new("Enable Hotkeys")
            {
                CheckOnClick = true,
                Checked = settingsManager.GetHotkeysEnabled()
            };
            enableHotkeysMenuItem.CheckedChanged += (s, e) => EnableHotkeysMenuItem_CheckedChanged(mainForm, settingsManager, s);
            settingsMenuItem.DropDownItems.Add(enableHotkeysMenuItem);

            settingsMenuItem.DropDownItems.Add(new ToolStripSeparator());

            // Layout Settings
            ToolStripMenuItem layoutMenuItem = new("Layout Settings");
            layoutMenuItem.Click += (s, e) => LayoutMenuItem_Click(mainForm, settingsManager);
            settingsMenuItem.DropDownItems.Add(layoutMenuItem);

            // Sorting Options
            ToolStripMenuItem sortingMenuItem = new("Sorting Options");
            sortingMenuItem.Click += (s, e) => SortingMenuItem_Click(mainForm, settingsManager);
            settingsMenuItem.DropDownItems.Add(sortingMenuItem);

            ToolStripMenuItem showSaveLocationMenuItem = new("Show Save Location");
            showSaveLocationMenuItem.Click += (s, e) => ShowSaveLocationMenuItem_Click(mainForm);
            settingsMenuItem.DropDownItems.Add(showSaveLocationMenuItem);

            settingsMenuItem.DropDownItems.Add(new ToolStripSeparator());

            AddSettingsBackupMenu(settingsMenuItem, settingsManager);
            AddRouteImportMenu(settingsMenuItem, mainForm);

            AppTheme.ApplyToMenuStrip(menuStrip);
        }

        // ==========MY NOTES==============
        // Adds the settings backup/restore submenu
        private static void AddSettingsBackupMenu(ToolStripMenuItem settingsMenuItem, SettingsManager settingsManager)
        {
            ToolStripMenuItem backupMenuItem = new("Settings Backup");

            // Restore from Backup option
            ToolStripMenuItem restoreMenuItem = new("Restore from Backup");
            restoreMenuItem.Click += (s, e) =>
            {
                var (hasBackup, backupDate, backupVersion) = settingsManager.GetBackupInfo();

                if (!hasBackup)
                {
                    MessageBox.Show("No backup file found.", "No Backup",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Restore settings from backup?\n\n" +
                    $"Backup Date: {backupDate:yyyy-MM-dd HH:mm}\n" +
                    $"Backup Version: {backupVersion}\n\n" +
                    "This will overwrite your current settings.",
                    "Restore Settings",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (settingsManager.RestoreFromBackup())
                    {
                        MessageBox.Show("Settings restored successfully!\n\nRestart the application to see all changes.",
                            "Settings Restored", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to restore settings from backup.", "Restore Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            };
            backupMenuItem.DropDownItems.Add(restoreMenuItem);

            // Show Backup Folder option
            ToolStripMenuItem showBackupFolderMenuItem = new("Show Backup Location");
            showBackupFolderMenuItem.Click += (s, e) => settingsManager.OpenBackupFolder();
            backupMenuItem.DropDownItems.Add(showBackupFolderMenuItem);

            // Show Downloaded Routes Folder option
            ToolStripMenuItem showDownloadedRoutesMenuItem = new("Show Downloaded Routes");
            showDownloadedRoutesMenuItem.Click += (s, e) =>
            {
                var historyManager = new RouteHistoryManager();
                string downloadFolder = historyManager.GetDownloadFolder();

                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", downloadFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open folder: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            backupMenuItem.DropDownItems.Add(showDownloadedRoutesMenuItem);

            settingsMenuItem.DropDownItems.Add(backupMenuItem);
        }

        private static void AddRouteImportMenu(ToolStripMenuItem settingsMenuItem, MainForm mainForm)
        {
            ToolStripMenuItem importRouteMenuItem = new("Import Route from URL");
            importRouteMenuItem.Click += (s, e) => ImportRouteMenuItem_Click(mainForm);
            settingsMenuItem.DropDownItems.Add(importRouteMenuItem);
        }

        private static void ImportRouteMenuItem_Click(MainForm mainForm)
        {
            bool wasTopMost = mainForm.TopMost;
            if (wasTopMost)
                mainForm.TopMost = false;

            try
            {
                ImportRouteForm.ShowImportDialog(mainForm);
            }
            finally
            {
                if (wasTopMost)
                    mainForm.TopMost = true;
            }
        }


        // ==========MY NOTES==============
        // Event handlers for settings menu items
        private static void AutoStartGameComboBox_SelectedIndexChanged(MainForm mainForm)
        {
            if (mainForm.autoStartGameComboBox == null || mainForm.enableAutoStartMenuItem == null)
                return;

            mainForm.enableAutoStartMenuItem.Enabled = mainForm.autoStartGameComboBox.SelectedItem != null;

            if (mainForm.enableAutoStartMenuItem.Checked && mainForm.autoStartGameComboBox.SelectedItem is string selectedGame)
            {
                Settings.Default.AutoStart = selectedGame;
                Settings.Default.Save();
            }
        }

        private static void EnableAutoStartMenuItem_CheckedChanged(MainForm mainForm)
        {
            if (mainForm.autoStartGameComboBox == null || mainForm.enableAutoStartMenuItem == null)
                return;

            if (mainForm.enableAutoStartMenuItem.Checked)
            {
                if (mainForm.autoStartGameComboBox.SelectedItem is string selectedGame)
                {
                    Settings.Default.AutoStart = selectedGame;
                }
            }
            else
            {
                Settings.Default.AutoStart = string.Empty;
            }
            Settings.Default.Save();
        }

        private static void ShowSaveLocationMenuItem_Click(MainForm mainForm)
        {
            string routeFilePath = mainForm.GetRouteManager()?.GetRouteFilePath() ??
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes", "AC4 100 % Route - Main Route.tsv");

            string saveDir = Path.Combine(
                Path.GetDirectoryName(routeFilePath) ?? AppDomain.CurrentDomain.BaseDirectory,
                "SavedProgress");

            MessageBox.Show($"Autosave location: {saveDir}",
                "Save File Location",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            try
            {
                Process.Start("explorer.exe", saveDir);
            }
            catch
            {
                MessageBox.Show($"Could not open the folder automatically.\n\n" +
                    $"Please navigate to this location manually:\n{saveDir}",
                    "Open Folder Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void HotkeysMenuItem_Click(MainForm mainForm, SettingsManager settingsManager)
        {
            HotkeysSettingsForm hotkeysForm = new(settingsManager)
            {
                StartPosition = FormStartPosition.CenterParent
            };

            bool wasTopMost = mainForm.TopMost;
            if (wasTopMost)
                mainForm.TopMost = false;

            hotkeysForm.ShowDialog(mainForm);

            if (wasTopMost)
                mainForm.TopMost = true;
        }

        private static void EnableHotkeysMenuItem_CheckedChanged(MainForm mainForm, SettingsManager settingsManager, object? sender)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                mainForm.SetHotkeysEnabled(menuItem.Checked);
                settingsManager.SaveHotkeysEnabled(menuItem.Checked);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private static void GameDirectoryMenuItem_Click(MainForm mainForm)
        {
            bool wasTopMost = mainForm.TopMost;
            if (wasTopMost)
                mainForm.TopMost = false;

            try
            {
                GameDirectoryForm gameDirectoryForm = new()
                {
                    Owner = mainForm,
                    StartPosition = FormStartPosition.CenterParent
                };

                gameDirectoryForm.DirectoryChanged += (s, args) => mainForm.RefreshAutoStartDropdownPublic();
                gameDirectoryForm.ShowDialog(mainForm);
            }
            finally
            {
                if (wasTopMost)
                    mainForm.TopMost = true;
            }
        }

        private static void AlwaysOnTopMenuItem_CheckedChanged(MainForm mainForm, SettingsManager settingsManager, object? sender)
        {
            if (sender is ToolStripMenuItem alwaysOnTopMenuItem)
            {
                mainForm.TopMost = alwaysOnTopMenuItem.Checked;
                settingsManager.SaveAlwaysOnTop(alwaysOnTopMenuItem.Checked);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private static void LayoutMenuItem_Click(MainForm mainForm, SettingsManager settingsManager)
        {
            bool wasTopMost = mainForm.TopMost;
            if (wasTopMost)
                mainForm.TopMost = false;

            try
            {
                using var layoutForm = new LayoutSettingsForm();
                layoutForm.LoadCurrentSettings(mainForm.GetCurrentLayoutMode());

                if (layoutForm.ShowDialog(mainForm) == DialogResult.OK)
                {
                    mainForm.SetCurrentLayoutMode(layoutForm.SelectedLayout);
                    LayoutManager.ApplyLayoutSettings(mainForm, layoutForm.SelectedLayout);

                    // Save settings
                    settingsManager.SaveLayoutMode(layoutForm.SelectedLayout);
                }
            }
            finally
            {
                if (wasTopMost)
                    mainForm.TopMost = true;
            }
        }

        // ==========MY NOTES==============
        // Updates the auto-start dropdown to only show games with folders set
        public static void RefreshAutoStartDropdown(MainForm mainForm, SettingsManager settingsManager)
        {
            if (mainForm.autoStartGameComboBox == null) return;

            string currentSelection = mainForm.autoStartGameComboBox.SelectedItem?.ToString() ?? string.Empty;
            mainForm.autoStartGameComboBox.Items.Clear();

            var gamesWithDirs = settingsManager.GetGamesWithDirectoriesSet();

            if (gamesWithDirs.Count > 0)
            {
                mainForm.autoStartGameComboBox.Items.AddRange([.. gamesWithDirs]);

                if (!string.IsNullOrEmpty(currentSelection) && gamesWithDirs.Contains(currentSelection))
                {
                    mainForm.autoStartGameComboBox.SelectedItem = currentSelection;
                }
            }

            if (mainForm.enableAutoStartMenuItem != null)
            {
                mainForm.enableAutoStartMenuItem.Enabled = mainForm.autoStartGameComboBox.Items.Count > 0;
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private static void SortingMenuItem_Click(MainForm mainForm, SettingsManager settingsManager)
        {
            bool wasTopMost = mainForm.TopMost;
            if (wasTopMost)
                mainForm.TopMost = false;

            try
            {
                using var sortingForm = new SortingOptionsForm(settingsManager);
                if (sortingForm.ShowDialog(mainForm) == DialogResult.OK)
                {
                    // Apply the new sorting mode immediately
                    var currentMode = settingsManager.GetSortingMode();
                    SortingManager.ApplySorting(mainForm.routeGrid, currentMode);
                }
            }
            finally
            {
                if (wasTopMost)
                    mainForm.TopMost = true;
            }
        }
    }
}