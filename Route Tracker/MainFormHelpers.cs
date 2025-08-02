using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Versioning;

namespace Route_Tracker
{
    public static class MainFormHelpers
    {
        #region UI Creation (from UICreationHelper.cs)
        // ==========MY NOTES==============
        // Sets up the main list that shows all route items and their completion status
        public static DataGridView CreateRouteGridView()
        {
            DataGridView routeGrid = new()
            {
                Name = "routeGrid",
                Dock = DockStyle.Fill,
                BackgroundColor = Color.Black,
                ForeColor = Color.White,
                GridColor = Color.FromArgb(80, 80, 80),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Font = new Font("Segoe UI", 11f),
                RowTemplate = { Height = 30 },
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            };

            ConfigureRouteGridColumns(routeGrid);
            ApplyRouteGridStyling(routeGrid);

            return routeGrid;
        }

        // ==========MY NOTES==============
        // Creates the two columns - one for the route description and one for the checkmark
        public static void ConfigureRouteGridColumns(DataGridView grid)
        {
            DataGridViewTextBoxColumn itemColumn = new()
            {
                Name = "Item",
                HeaderText = "",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Padding = new Padding(10, 5, 5, 5),
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    WrapMode = DataGridViewTriState.True
                }
            };

            DataGridViewTextBoxColumn completedColumn = new()
            {
                Name = "Completed",
                HeaderText = "",
                Width = 50,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    BackColor = Color.Black,
                    ForeColor = Color.White
                }
            };

            grid.Columns.Add(itemColumn);
            grid.Columns.Add(completedColumn);
        }

        // ==========MY NOTES==============
        // Makes the route grid look good with our dark theme
        public static void ApplyRouteGridStyling(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.RowHeadersDefaultCellStyle.BackColor = Color.Black;
            grid.RowsDefaultCellStyle.BackColor = Color.Black;
            grid.RowsDefaultCellStyle.ForeColor = Color.White;

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.Black;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 30, 35);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;

            grid.ColumnHeadersHeight = 4;
            grid.ColumnHeadersVisible = false;
            grid.ScrollBars = ScrollBars.Vertical;

            // Set background explicitly
            grid.BackgroundColor = Color.Black;
        }
        #endregion

        #region Context Menu (from ContextMenuHelper.cs)
        // ==========MY NOTES==============
        // Creates the right-click menu with different options depending on what's loaded
        public static ContextMenuStrip? CreateDynamicContextMenu(MainForm mainForm, RouteManager? routeManager)
        {
            var contextMenu = new ContextMenuStrip
            {
                BackColor = AppTheme.BackgroundColor,
                ForeColor = AppTheme.TextColor,
                ShowImageMargin = false,
                ShowCheckMargin = false,
                DropShadowEnabled = false,
                RenderMode = ToolStripRenderMode.System
            };

            // Load Route File... (always available)
            var loadRouteFileMenuItem = new ToolStripMenuItem("Load Route File...")
            {
                BackColor = AppTheme.BackgroundColor,
                ForeColor = AppTheme.TextColor
            };
            loadRouteFileMenuItem.Click += (s, e) => LoadRouteFile(mainForm);
            contextMenu.Items.Add(loadRouteFileMenuItem);

            if (routeManager != null)
            {
                contextMenu.Items.Add(new ToolStripSeparator());

                var saveProgressMenuItem = new ToolStripMenuItem("Save Progress...")
                {
                    BackColor = AppTheme.BackgroundColor,
                    ForeColor = AppTheme.TextColor
                };
                saveProgressMenuItem.Click += (s, e) => routeManager.SaveProgress(mainForm);

                var loadProgressMenuItem = new ToolStripMenuItem("Load Progress...")
                {
                    BackColor = AppTheme.BackgroundColor,
                    ForeColor = AppTheme.TextColor
                };
                loadProgressMenuItem.Click += (s, e) => LoadProgress(mainForm);

                var resetProgressMenuItem = new ToolStripMenuItem("Reset Progress...")
                {
                    BackColor = AppTheme.BackgroundColor,
                    ForeColor = AppTheme.TextColor
                };
                resetProgressMenuItem.Click += (s, e) => ResetProgress(mainForm, routeManager);

                contextMenu.Items.Add(saveProgressMenuItem);
                contextMenu.Items.Add(loadProgressMenuItem);
                contextMenu.Items.Add(resetProgressMenuItem);
            }
            return contextMenu;
        }

        // ==========MY NOTES==============
        // Shows the right-click menu when you right-click on the route list
        public static void HandleRouteGridMouseClick(MainForm mainForm, RouteManager? routeManager, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var contextMenu = CreateDynamicContextMenu(mainForm, routeManager);
                contextMenu?.Show(mainForm.routeGrid, e.Location);
            }
        }

        // ==========MY NOTES==============
        // Lets you pick a different route file to load from the right-click menu
        public static void LoadRouteFile(MainForm mainForm)
        {
            try
            {
                using var openDialog = new OpenFileDialog
                {
                    Filter = "Route Files (*.tsv)|*.tsv|All Files (*.*)|*.*",
                    Title = "Load Route File",
                    CheckFileExists = true
                };

                try
                {
                    string routesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Routes");
                    if (Directory.Exists(routesFolder))
                    {
                        openDialog.InitialDirectory = routesFolder;
                    }
                }
                catch (Exception ex)
                {
                    LoggingSystem.LogError("Could not access Routes folder", ex);
                }

                if (openDialog.ShowDialog(mainForm) == DialogResult.OK)
                {
                    LoadingHelper.ExecuteWithSpinner(mainForm, () =>
                    {
                        mainForm.SetRouteManager(new RouteManager(openDialog.FileName, mainForm.GameConnectionManager));
                        mainForm.LoadRouteDataPublic();
                        mainForm.UpdateCurrentLocationLabel();

                        string fileName = Path.GetFileName(openDialog.FileName);
                        MessageBox.Show($"Route loaded successfully: {fileName}", "Route Loaded",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }, "Loading Route File...");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError("Failed to load route file", ex);
                MessageBox.Show($"Error loading route file: {ex.Message}", "Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========MY NOTES==============
        // Loads progress from saved file
        public static void LoadProgress(MainForm mainForm)
        {
            try
            {
                RouteManager? routeManager = mainForm.GetRouteManager();

                routeManager ??= mainForm.CreateDefaultRouteManager();

                if (routeManager.LoadEntries().Count == 0)
                {
                    LoggingSystem.LogError("No route entries found when trying to load progress");
                    MessageBox.Show("No route entries found. Make sure the route file exists.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LoadingHelper.ExecuteWithSpinner(mainForm, () =>
                {
                    if (routeManager.LoadProgress(mainForm.routeGrid, mainForm))
                    {
                        MessageBox.Show("Progress loaded successfully.", "Load Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }, "Loading Progress...");
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError("Failed to load progress", ex);
                MessageBox.Show($"Error loading progress: {ex.Message}", "Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========MY NOTES==============
        // Resets progress with confirmation
        public static void ResetProgress(MainForm mainForm, RouteManager? routeManager)
        {
            try
            {
                if (routeManager == null)
                {
                    routeManager = mainForm.CreateDefaultRouteManager();
                    if (routeManager.LoadEntries().Count == 0)
                    {
                        LoggingSystem.LogError("No route entries found when trying to reset progress");
                        MessageBox.Show("No route entries found. Make sure the route file exists.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                var result = MessageBox.Show(
                    "Are you sure you want to reset your progress?\n\nThis will delete your autosave and cannot be undone.",
                    "Reset Progress",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    LoadingHelper.ExecuteWithSpinner(mainForm, () =>
                    {
                        routeManager.ResetProgress(mainForm.routeGrid);
                        MessageBox.Show("Progress has been reset.", "Reset Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }, "Resetting Progress...");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.LogError("Failed to reset progress", ex);
                MessageBox.Show($"Error resetting progress: {ex.Message}", "Reset Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Hotkey Processing (from HotkeyManager.cs)
        // ==========MY NOTES==============
        // Enhanced hotkey processing with global hotkeys and advanced mode support
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public static bool ProcessCmdKey(MainForm mainForm, SettingsManager settingsManager, RouteManager? routeManager, ref Message msg, Keys keyData)
        {
            if (!mainForm.IsHotkeysEnabled)
                return false;

            var hotkeySettings = settingsManager.GetAllHotkeySettings();

            // Skip processing if global hotkeys are enabled (handled by WndProc)
            if (hotkeySettings.GlobalHotkeys)
                return false;

            return ProcessHotkeyAction(mainForm, routeManager, keyData, hotkeySettings);
        }

        // ==========MY NOTES==============
        // Processes hotkey actions based on current settings and mode
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        private static bool ProcessHotkeyAction(MainForm mainForm, RouteManager? routeManager, Keys keyData, (Keys CompleteHotkey, Keys SkipHotkey, Keys UndoHotkey, bool GlobalHotkeys, bool AdvancedHotkeys) settings)
        {
            if (routeManager == null) return false;

            RouteEntry? targetEntry = null;

            // Determine which entry to act on based on advanced hotkeys setting
            if (settings.AdvancedHotkeys)
            {
                targetEntry = routeManager.GetSelectedEntry(mainForm.routeGrid);
                if (targetEntry == null) return false; // No selection in advanced mode
            }
            else
            {
                targetEntry = routeManager.GetFirstIncompleteEntry(mainForm.routeGrid);
                if (targetEntry == null) return false; // No incomplete entries in normal mode
            }

            // Process the specific hotkey
            if (keyData == settings.CompleteHotkey)
            {
                ProcessCompleteAction(mainForm, routeManager, targetEntry, settings.AdvancedHotkeys);
                return true;
            }
            else if (keyData == settings.SkipHotkey)
            {
                ProcessSkipAction(mainForm, routeManager, targetEntry, settings.AdvancedHotkeys);
                return true;
            }
            else if (keyData == settings.UndoHotkey)
            {
                ProcessUndoAction(mainForm, routeManager, targetEntry, settings.AdvancedHotkeys);
                return true;
            }

            return false;
        }

        // ==========MY NOTES==============
        // Processes complete action with mode-specific behavior
        private static void ProcessCompleteAction(MainForm mainForm, RouteManager routeManager, RouteEntry targetEntry, bool advancedMode)
        {
            if (advancedMode)
            {
                // In advanced mode, can complete any entry (even if already completed)
                routeManager.CompleteEntry(mainForm.routeGrid, targetEntry);
            }
            else
            {
                // In normal mode, only complete if not already completed
                if (!targetEntry.IsCompleted)
                {
                    routeManager.CompleteEntry(mainForm.routeGrid, targetEntry);
                }
            }

            UpdateCompletionStats(mainForm, routeManager);
            mainForm.UpdateCurrentLocationLabel();
        }

        // ==========MY NOTES==============
        // Processes skip action with mode-specific behavior
        private static void ProcessSkipAction(MainForm mainForm, RouteManager routeManager, RouteEntry targetEntry, bool advancedMode)
        {
            if (advancedMode)
            {
                // In advanced mode, can skip any entry (even if already skipped)
                if (!targetEntry.IsSkipped)
                {
                    routeManager.SkipEntry(mainForm.routeGrid, targetEntry);
                }
            }
            else
            {
                // In normal mode, only skip if not already completed or skipped
                if (!targetEntry.IsCompleted && !targetEntry.IsSkipped)
                {
                    routeManager.SkipEntry(mainForm.routeGrid, targetEntry);
                }
            }

            UpdateCompletionStats(mainForm, routeManager);
            mainForm.UpdateCurrentLocationLabel();
        }

        // ==========MY NOTES==============
        // Processes undo action - works on any completed or skipped entry
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        private static void ProcessUndoAction(MainForm mainForm, RouteManager routeManager, RouteEntry targetEntry, bool advancedMode)
        {
            // Undo works if the entry is completed or skipped
            if (targetEntry.IsCompleted || targetEntry.IsSkipped)
            {
                routeManager.UndoEntry(mainForm.routeGrid, targetEntry);
                UpdateCompletionStats(mainForm, routeManager);
                mainForm.UpdateCurrentLocationLabel();
            }
        }

        // ==========MY NOTES==============
        // Updates completion statistics after hotkey actions
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        private static void UpdateCompletionStats(MainForm mainForm, RouteManager routeManager)
        {
            var (percentage, completed, total) = routeManager.CalculateCompletionStats();
            mainForm.completionLabel.Text = $"Completion: {percentage:F2}%";
            RouteHelpers.UpdateCompletionStatsIfVisible(routeManager);
        }

        // ==========MY NOTES==============
        // Public method for global hotkey processing
        public static void ProcessGlobalHotkey(MainForm mainForm, SettingsManager settingsManager, RouteManager? routeManager, Keys keyPressed)
        {
            var hotkeySettings = settingsManager.GetAllHotkeySettings();
            ProcessHotkeyAction(mainForm, routeManager, keyPressed, hotkeySettings);
        }
        #endregion
    }
}