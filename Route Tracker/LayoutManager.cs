using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Route_Tracker
{
    public static class LayoutManager
    {
        #region Font Caching System
        // ==========MY NOTES==============
        // Font cache to prevent memory leaks and improve performance
        // Reuses font objects instead of creating new ones every time
        private static readonly Dictionary<float, Font> _fontCache = [];
        private static readonly Dictionary<float, Font> _headerFontCache = [];

        // Control group cache for faster visibility switching
        private static readonly Dictionary<string, Control[]> _controlGroups = [];

        // ==========MY NOTES==============
        // Gets or creates a cached font for the given scale factor
        // Prevents creating duplicate font objects and memory leaks
        private static Font GetScaledFont(float scale)
        {
            if (!_fontCache.TryGetValue(scale, out Font? font))
            {
                font = new Font(AppTheme.DefaultFont.FontFamily, AppTheme.DefaultFont.Size * scale);
                _fontCache[scale] = font;
            }
            return font;
        }

        // ==========MY NOTES==============
        // Gets or creates a cached header font for the given scale factor
        private static Font GetScaledHeaderFont(float scale)
        {
            if (!_headerFontCache.TryGetValue(scale, out Font? font))
            {
                font = new Font(AppTheme.DefaultFont.FontFamily, (AppTheme.DefaultFont.Size + 2) * scale);
                _headerFontCache[scale] = font;
            }
            return font;
        }

        // ==========MY NOTES==============
        // Cleanup method to dispose cached fonts when app closes
        public static void DisposeCache()
        {
            foreach (var font in _fontCache.Values)
                font?.Dispose();
            foreach (var font in _headerFontCache.Values)
                font?.Dispose();

            _fontCache.Clear();
            _headerFontCache.Clear();
            _controlGroups.Clear();
        }
        #endregion

        #region Layout Configuration
        // ==========MY NOTES==============
        // Layout configuration data to avoid hardcoding values everywhere
        private static readonly Dictionary<LayoutSettingsForm.LayoutMode, LayoutConfig> _layoutConfigs = new()
        {
            [LayoutSettingsForm.LayoutMode.Normal] = new LayoutConfig
            {
                Size = new Size(800, 600),
                MinSize = new Size(400, 200),
                FontScale = 1.0f,
                BorderStyle = FormBorderStyle.Sizable,
                ShowAllControls = true,
                HideControls = []
            },
            [LayoutSettingsForm.LayoutMode.Compact] = new LayoutConfig
            {
                Size = new Size(700, 500),
                MinSize = new Size(500, 300),
                FontScale = 0.9f,
                BorderStyle = FormBorderStyle.Sizable,
                ShowAllControls = true,
                HideControls = ["searchTextBox", "typeFilterPanel", "clearFiltersButton"]  // Hide the panel instead
            },
            [LayoutSettingsForm.LayoutMode.Mini] = new LayoutConfig
            {
                Size = new Size(400, 300),
                MinSize = new Size(300, 200),
                FontScale = 0.8f,
                BorderStyle = FormBorderStyle.Sizable,
                ShowAllControls = false,
                HideControls = []
            },
            [LayoutSettingsForm.LayoutMode.Overlay] = new LayoutConfig
            {
                Size = new Size(280, 800),  // Reduced width for better overlay fit
                MinSize = new Size(200, 400),  // Reduced minimum width
                FontScale = 0.8f,  // Smaller font scale for overlay mode
                BorderStyle = FormBorderStyle.SizableToolWindow,
                ShowAllControls = false,
                HideControls = ["searchTextBox", "typeFilterPanel", "clearFiltersButton"]  // Hide the panel instead
            }
        };

        // ==========MY NOTES==============
        // Configuration class for layout settings
        private class LayoutConfig
        {
            public Size Size { get; set; }
            public Size MinSize { get; set; }
            public float FontScale { get; set; }
            public FormBorderStyle BorderStyle { get; set; }
            public bool ShowAllControls { get; set; }
            public string[] HideControls { get; set; } = [];
        }
        #endregion

        #region Public API
        // ==========MY NOTES==============
        // Apply layout mode scaling/visibility - OPTIMIZED VERSION
        public static void ApplyLayoutSettings(MainForm mainForm, LayoutSettingsForm.LayoutMode currentLayoutMode)
        {
            ApplyLayoutMode(mainForm, currentLayoutMode);
        }

        // ==========MY NOTES==============
        // Handles all the different layout modes - DRAMATICALLY OPTIMIZED
        public static void ApplyLayoutMode(MainForm mainForm, LayoutSettingsForm.LayoutMode currentLayoutMode)
        {
            if (!_layoutConfigs.TryGetValue(currentLayoutMode, out LayoutConfig? config))
                return;

            // Cache control groups for faster access
            CacheControlGroups(mainForm);

            // Apply configuration in optimal order
            ApplyFormSettings(mainForm, config, currentLayoutMode);
            ApplyControlVisibility(mainForm, config);
            ApplyFontScaling(mainForm, config.FontScale);
            ApplyLabelPositioning(mainForm, currentLayoutMode);
        }
        #endregion

        #region Optimized Implementation Methods
        // ==========MY NOTES==============
        // Cache frequently accessed control groups to avoid repeated searches
        private static void CacheControlGroups(MainForm mainForm)
        {
            if (_controlGroups.Count > 0) return; // Already cached

            _controlGroups["buttons"] =
            [
                mainForm.showStatsButton,
                mainForm.showCompletionButton,
                mainForm.clearFiltersButton
            ];

            // Find the typeFilterPanel (parent of typeFilterCheckedListBox)
            var typeFilterPanel = mainForm.typeFilterCheckedListBox?.Parent;

            // Create filters array with only non-null controls
            var filterControls = new List<Control>();

            if (mainForm.searchTextBox != null)
                filterControls.Add(mainForm.searchTextBox);

            if (typeFilterPanel != null)
                filterControls.Add(typeFilterPanel);
            else if (mainForm.typeFilterCheckedListBox != null)
                filterControls.Add(mainForm.typeFilterCheckedListBox);

            _controlGroups["filters"] = [.. filterControls];

            _controlGroups["essential"] =
            [
                mainForm.completionLabel,
                mainForm.currentLocationLabel,
                mainForm.routeGrid
            ];

            _controlGroups["labels"] =
            [
                mainForm.completionLabel,
                mainForm.currentLocationLabel
            ];
        }

        // ==========MY NOTES==============
        // Apply form-level settings efficiently
        private static void ApplyFormSettings(MainForm mainForm, LayoutConfig config, LayoutSettingsForm.LayoutMode mode)
        {
            // Set minimum size first to prevent conflicts
            mainForm.MinimumSize = config.MinSize;
            mainForm.Size = config.Size;
            mainForm.FormBorderStyle = config.BorderStyle;

            // Special handling for overlay mode
            if (mode == LayoutSettingsForm.LayoutMode.Overlay)
            {
                mainForm.MaximizeBox = false;
                mainForm.MinimizeBox = false;
                mainForm.ControlBox = true;
            }
            else
            {
                mainForm.MaximizeBox = true;
                mainForm.MinimizeBox = true;
                mainForm.ControlBox = true;
            }
        }

        // ==========MY NOTES==============
        // Apply control visibility using cached control groups
        private static void ApplyControlVisibility(MainForm mainForm, LayoutConfig config)
        {
            if (config.ShowAllControls)
            {
                // Show all controls efficiently
                SetControlGroupVisibility(_controlGroups["buttons"], true);
                SetControlGroupVisibility(_controlGroups["filters"], true);
                SetControlGroupVisibility(_controlGroups["essential"], true);

                // Hide specific controls for compact mode
                foreach (string controlName in config.HideControls)
                {
                    var control = GetControlByName(mainForm, controlName);
                    if (control != null)
                    {
                        control.Visible = false;
                        Debug.WriteLine($"Hiding control: {controlName} (Type: {control.GetType().Name})");
                    }
                    else
                    {
                        Debug.WriteLine($"Control not found: {controlName}");
                    }
                }
            }
            else
            {
                // Hide most controls, show only essential
                SetControlGroupVisibility(_controlGroups["buttons"], false);
                SetControlGroupVisibility(_controlGroups["filters"], false);
                SetControlGroupVisibility(_controlGroups["essential"], true);

                // Explicitly hide any controls specified in HideControls (for overlay mode)
                foreach (string controlName in config.HideControls)
                {
                    var control = GetControlByName(mainForm, controlName);
                    if (control != null)
                    {
                        control.Visible = false;
                        Debug.WriteLine($"Explicitly hiding control: {controlName} (Type: {control.GetType().Name})");
                    }
                    else
                    {
                        Debug.WriteLine($"Control not found for explicit hiding: {controlName}");
                    }
                }
            }
        }

        // ==========MY NOTES==============
        // Efficiently set visibility for a group of controls
        private static void SetControlGroupVisibility(Control[] controls, bool visible)
        {
            foreach (var control in controls)
            {
                if (control != null)
                    control.Visible = visible;
            }
        }

        // ==========MY NOTES==============
        // Get control by name without recursive search
        private static Control? GetControlByName(MainForm mainForm, string name)
        {
            return name switch
            {
                "searchTextBox" => mainForm.searchTextBox,
                "typeFilterCheckedListBox" => mainForm.typeFilterCheckedListBox?.Parent ?? mainForm.typeFilterCheckedListBox, // Return the panel instead
                "typeFilterPanel" => mainForm.typeFilterCheckedListBox?.Parent,
                "clearFiltersButton" => mainForm.clearFiltersButton,
                "showStatsButton" => mainForm.showStatsButton,
                "showCompletionButton" => mainForm.showCompletionButton,
                "completionLabel" => mainForm.completionLabel,
                "currentLocationLabel" => mainForm.currentLocationLabel,
                _ => null
            };
        }

        // ==========MY NOTES==============
        // Apply font scaling using cached fonts - MUCH MORE EFFICIENT
        private static void ApplyFontScaling(MainForm mainForm, float fontScale)
        {
            // Get cached fonts
            Font scaledFont = GetScaledFont(fontScale);
            Font scaledHeaderFont = GetScaledHeaderFont(fontScale);

            // Apply to specific control types efficiently
            ApplyFontToControlGroup(_controlGroups["buttons"], scaledFont);

            // Apply to completion label with header font
            if (mainForm.completionLabel != null)
                mainForm.completionLabel.Font = scaledHeaderFont;

            // Apply to current location label with header font (FIXED)
            if (mainForm.currentLocationLabel != null)
                mainForm.currentLocationLabel.Font = scaledHeaderFont;

            // Apply to route grid with optimized settings
            if (mainForm.routeGrid != null)
            {
                mainForm.routeGrid.Font = new Font("Segoe UI", 11f * fontScale);
                mainForm.routeGrid.RowTemplate.Height = (int)(30 * fontScale);
            }

            // Apply to search controls
            if (mainForm.searchTextBox != null)
                mainForm.searchTextBox.Font = scaledFont;
        }

        // ==========MY NOTES==============
        // Apply font to a group of controls efficiently
        private static void ApplyFontToControlGroup(Control[] controls, Font font)
        {
            foreach (var control in controls)
            {
                if (control is Label or Button)
                    control.Font = font;
            }
        }
        #endregion

        #region Legacy Methods (Simplified)
        // ==========MY NOTES==============
        // Simplified legacy method for backwards compatibility
        public static void ShowAllControls(MainForm mainForm, bool show)
        {
            var config = show ? _layoutConfigs[LayoutSettingsForm.LayoutMode.Normal]
                             : _layoutConfigs[LayoutSettingsForm.LayoutMode.Mini];
            ApplyControlVisibility(mainForm, config);
        }

        // ==========MY NOTES==============
        // Legacy method - now just calls optimized font scaling
        public static void ScaleInterface(MainForm mainForm, float scaleFactor)
        {
            ApplyFontScaling(mainForm, scaleFactor);
        }

        // ==========MY NOTES==============
        // Legacy method - replaced with cached control groups
        [Obsolete("Use cached control groups instead")]
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
        #endregion

        // ==========MY NOTES==============
        // Apply layout-specific positioning for labels in different modes
        private static void ApplyLabelPositioning(MainForm mainForm, LayoutSettingsForm.LayoutMode currentLayoutMode)
        {
            if (mainForm.completionLabel == null || mainForm.currentLocationLabel == null)
                return;

            // Get the parent panel
            var labelPanel = mainForm.completionLabel.Parent;
            if (labelPanel == null)
                return;

            switch (currentLayoutMode)
            {
                case LayoutSettingsForm.LayoutMode.Overlay:
                    // For overlay mode, use tight vertical stacking with minimal spacing
                    mainForm.completionLabel.Location = new Point(5, 2);  // Closer to edge, minimal top padding

                    // Calculate actual text height instead of full control height
                    int textHeight = (int)(mainForm.completionLabel.Font.Height);
                    mainForm.currentLocationLabel.Location = new Point(0, 2 + textHeight + 1);  // Just 1px gap between text lines

                    // Make panel just tall enough for both labels with minimal padding
                    if (labelPanel is Panel panel)
                    {
                        panel.Height = (textHeight * 2) + 6;  // Just enough space for both lines + small padding
                    }
                    break;

                case LayoutSettingsForm.LayoutMode.Mini:
                    // For mini mode, stack vertically but with slightly more breathing room
                    mainForm.completionLabel.Location = new Point(10, 0);
                    mainForm.currentLocationLabel.Location = new Point(10, mainForm.completionLabel.Height + 2);

                    if (labelPanel is Panel miniPanel)
                    {
                        miniPanel.Height = (mainForm.completionLabel.Height * 2) + 8;
                    }
                    break;

                case LayoutSettingsForm.LayoutMode.Compact:
                    // For compact mode, reduce spacing between labels
                    mainForm.completionLabel.Location = new Point(10, 0);
                    mainForm.currentLocationLabel.Location = new Point(mainForm.completionLabel.Right + 15, 0);
                    break;

                case LayoutSettingsForm.LayoutMode.Normal:
                default:
                    // Default horizontal positioning with generous spacing
                    mainForm.completionLabel.Location = new Point(10, 0);
                    mainForm.currentLocationLabel.Location = new Point(mainForm.completionLabel.Right + 30, 0);
                    break;
            }

            Debug.WriteLine($"Applied label positioning for {currentLayoutMode} mode - Completion: {mainForm.completionLabel.Location}, Current: {mainForm.currentLocationLabel.Location}");
        }
    }
}