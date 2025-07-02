using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Route_Tracker
{
    public static class LayoutManager
    {
        // ==========MY NOTES==============
        // Apply layout mode scaling/visibility
        public static void ApplyLayoutSettings(MainForm mainForm, LayoutSettingsForm.LayoutMode currentLayoutMode)
        {
            ApplyLayoutMode(mainForm, currentLayoutMode);
        }

        // ==========MY NOTES==============
        // Handles all the different layout modes and their settings
        public static void ApplyLayoutMode(MainForm mainForm, LayoutSettingsForm.LayoutMode currentLayoutMode)
        {
            switch (currentLayoutMode)
            {
                case LayoutSettingsForm.LayoutMode.Normal:
                    // Restore to default normal size
                    ShowAllControls(mainForm, true);
                    ScaleInterface(mainForm, 1.0f);
                    mainForm.FormBorderStyle = FormBorderStyle.Sizable;
                    mainForm.MinimumSize = new Size(600, 200);
                    mainForm.Size = new Size(800, 600); // Set default normal size
                    break;

                case LayoutSettingsForm.LayoutMode.Compact:
                    // Set default compact size
                    ShowAllControls(mainForm, true);
                    mainForm.searchTextBox.Visible = false;
                    mainForm.typeFilterComboBox.Visible = false;
                    mainForm.clearFiltersButton.Visible = false;
                    ScaleInterface(mainForm, 0.9f);
                    mainForm.FormBorderStyle = FormBorderStyle.Sizable;
                    mainForm.Size = new Size(700, 500); // Set default compact size
                    break;

                case LayoutSettingsForm.LayoutMode.Mini:
                    // Set default mini size
                    ShowAllControls(mainForm, false);
                    mainForm.completionLabel.Visible = true;
                    mainForm.routeGrid.Visible = true;
                    ScaleInterface(mainForm, 0.8f);
                    mainForm.FormBorderStyle = FormBorderStyle.Sizable;
                    mainForm.Size = new Size(400, 300); // Set default mini size
                    break;

                case LayoutSettingsForm.LayoutMode.Overlay:
                    // Set default overlay size - LONG VERTICAL RECTANGLE
                    ShowAllControls(mainForm, false);
                    mainForm.completionLabel.Visible = true;
                    mainForm.routeGrid.Visible = true;
                    ScaleInterface(mainForm, 0.7f);

                    //set minimum size first for overlay
                    mainForm.MinimumSize = new Size(100, 400);
                    mainForm.Size = new Size(300, 800); // Set default overlay size

                    // Use ToolWindow style for smaller title bar and X button
                    mainForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                    mainForm.MaximizeBox = false;
                    mainForm.MinimizeBox = false;
                    mainForm.ControlBox = true; // Keep tiny X button

                    break;
            }
        }

        // ==========MY NOTES==============
        // Shows or hides controls based on layout mode
        public static void ShowAllControls(MainForm mainForm, bool show)
        {
            mainForm.showStatsButton.Visible = show;
            mainForm.showCompletionButton.Visible = show;
            mainForm.searchTextBox.Visible = show;
            mainForm.typeFilterComboBox.Visible = show;
            mainForm.clearFiltersButton.Visible = show;

            // Don't override size here anymore - handled in ApplyLayoutMode
            if (show && mainForm.GetCurrentLayoutMode() == LayoutSettingsForm.LayoutMode.Normal)
            {
                mainForm.FormBorderStyle = FormBorderStyle.Sizable;
            }
        }

        // ==========MY NOTES==============
        // Scales fonts and UI elements based on layout mode
        public static void ScaleInterface(MainForm mainForm, float scaleFactor)
        {
            // Scale fonts
            var scaledDefaultFont = new Font(AppTheme.DefaultFont.FontFamily, AppTheme.DefaultFont.Size * scaleFactor);

            foreach (Control control in GetAllControls(mainForm))
            {
                if (control is Label || control is Button)
                {
                    control.Font = scaledDefaultFont;
                }
            }

            // Scale route grid row height
            if (mainForm.routeGrid != null)
            {
                mainForm.routeGrid.RowTemplate.Height = (int)(30 * scaleFactor);
                mainForm.routeGrid.Font = new Font("Segoe UI", 11f * scaleFactor);
            }
        }

        // ==========MY NOTES==============
        // Gets all controls recursively for font scaling
        public static IEnumerable<Control> GetAllControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                yield return control;
                foreach (Control child in GetAllControls(control))
                {
                    yield return child;
                }
            }
        }
    }
}