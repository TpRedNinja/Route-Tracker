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
        private static void LoadRouteFile(MainForm mainForm)
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
                catch { /* Ignore if we can't access the folder */ }

                if (openDialog.ShowDialog(mainForm) == DialogResult.OK)
                {
                    mainForm.SetRouteManager(new RouteManager(openDialog.FileName, mainForm.GameConnectionManager));
                    mainForm.LoadRouteDataPublic();

                    string fileName = Path.GetFileName(openDialog.FileName);
                    MessageBox.Show($"Route loaded successfully: {fileName}", "Route Loaded",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading route file: {ex.Message}", "Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========MY NOTES==============
        // Loads progress from saved file
        private static void LoadProgress(MainForm mainForm)
        {
            RouteManager? routeManager = mainForm.GetRouteManager();

            routeManager ??= mainForm.CreateDefaultRouteManager();

            if (routeManager.LoadEntries().Count == 0)
            {
                MessageBox.Show("No route entries found. Make sure the route file exists.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (routeManager.LoadProgress(mainForm.routeGrid, mainForm))
            {
                MessageBox.Show("Progress loaded successfully.", "Load Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ==========MY NOTES==============
        // Resets progress with confirmation
        private static void ResetProgress(MainForm mainForm, RouteManager routeManager)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset your progress?\n\nThis will delete your autosave and cannot be undone.",
                "Reset Progress",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                routeManager.ResetProgress(mainForm.routeGrid);
            }
        }
        #endregion

        #region Hotkey Processing (from HotkeyManager.cs)
        // ==========MY NOTES==============
        // Catches key presses and runs hotkey actions if hotkeys are enabled
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public static bool ProcessCmdKey(MainForm mainForm, SettingsManager settingsManager, RouteManager? routeManager, ref Message msg, Keys keyData)
        {
            if (!mainForm.IsHotkeysEnabled)
                return false;

            (Keys CompleteHotkey, Keys SkipHotkey) = settingsManager.GetHotkeys();

            if (keyData == CompleteHotkey)
            {
                CompleteSelectedEntry(mainForm, routeManager);
                return true;
            }
            else if (keyData == SkipHotkey)
            {
                SkipSelectedEntry(mainForm, routeManager);
                return true;
            }

            return false;
        }

        // ==========MY NOTES==============
        // Marks the selected route entry as done when you press the complete hotkey
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        private static void CompleteSelectedEntry(MainForm mainForm, RouteManager? routeManager)
        {
            if (mainForm.routeGrid.CurrentRow != null && mainForm.routeGrid.CurrentRow.Tag is RouteEntry selectedEntry)
            {
                selectedEntry.IsCompleted = true;
                mainForm.routeGrid.CurrentRow.Cells[1].Value = "X";

                RouteManager.SortRouteGridByCompletion(mainForm.routeGrid);
                RouteManager.ScrollToFirstIncomplete(mainForm.routeGrid);

                routeManager?.AutoSaveProgress();

                if (routeManager != null)
                {
                    var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                    mainForm.completionLabel.Text = $"Completion: {percentage:F2}%";

                    RouteHelpers.UpdateCompletionStatsIfVisible(routeManager);
                }
            }
        }

        // ==========MY NOTES==============
        // Skips the selected route entry when you press the skip hotkey
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        private static void SkipSelectedEntry(MainForm mainForm, RouteManager? routeManager)
        {
            if (mainForm.routeGrid.CurrentRow != null && mainForm.routeGrid.CurrentRow.Tag is RouteEntry selectedEntry)
            {
                selectedEntry.IsSkipped = true;
                mainForm.routeGrid.Rows.Remove(mainForm.routeGrid.CurrentRow);

                routeManager?.AutoSaveProgress();

                if (routeManager != null)
                {
                    var (percentage, completed, total) = routeManager.CalculateCompletionStats();
                    mainForm.completionLabel.Text = $"Completion: {percentage:F2}%";

                    RouteHelpers.UpdateCompletionStatsIfVisible(routeManager);
                }
            }
        }
        #endregion
    }
}