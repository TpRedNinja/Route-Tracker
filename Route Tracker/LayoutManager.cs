using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<float, Font> _fontCache = new();
        private static readonly Dictionary<float, Font> _headerFontCache = new();

        // Control group cache for faster visibility switching
        private static readonly Dictionary<string, Control[]> _controlGroups = new();

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
                MinSize = new Size(600, 200),
                FontScale = 1.0f,
                BorderStyle = FormBorderStyle.Sizable,
                ShowAllControls = true,
                HideControls = Array.Empty<string>()
            },
            [LayoutSettingsForm.LayoutMode.Compact] = new LayoutConfig
            {
                Size = new Size(700, 500),
                MinSize = new Size(500, 300),
                FontScale = 0.9f,
                BorderStyle = FormBorderStyle.Sizable,
                ShowAllControls = true,
                HideControls = new[] { "searchTextBox", "typeFilterCheckedListBox", "clearFiltersButton" }
            },
            [LayoutSettingsForm.LayoutMode.Mini] = new LayoutConfig
            {
                Size = new Size(400, 300),
                MinSize = new Size(300, 200),
                FontScale = 0.8f,
                BorderStyle = FormBorderStyle.Sizable,
                ShowAllControls = false,
                HideControls = Array.Empty<string>()
            },
            [LayoutSettingsForm.LayoutMode.Overlay] = new LayoutConfig
            {
                Size = new Size(300, 800),
                MinSize = new Size(100, 400),
                FontScale = 0.7f,
                BorderStyle = FormBorderStyle.SizableToolWindow,
                ShowAllControls = false,
                HideControls = Array.Empty<string>()
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
            public string[] HideControls { get; set; } = Array.Empty<string>();
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
        }
        #endregion

        #region Optimized Implementation Methods
        // ==========MY NOTES==============
        // Cache frequently accessed control groups to avoid repeated searches
        private static void CacheControlGroups(MainForm mainForm)
        {
            if (_controlGroups.Count > 0) return; // Already cached

            _controlGroups["buttons"] = new Control[]
            {
                mainForm.showStatsButton,
                mainForm.showCompletionButton,
                mainForm.clearFiltersButton
            };

            _controlGroups["filters"] = new Control[]
            {
                mainForm.searchTextBox,
                mainForm.typeFilterCheckedListBox
            };

            _controlGroups["essential"] = new Control[]
            {
                mainForm.completionLabel,
                mainForm.routeGrid
            };
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
                        control.Visible = false;
                }
            }
            else
            {
                // Hide most controls, show only essential
                SetControlGroupVisibility(_controlGroups["buttons"], false);
                SetControlGroupVisibility(_controlGroups["filters"], false);
                SetControlGroupVisibility(_controlGroups["essential"], true);
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
                "typeFilterCheckedListBox" => mainForm.typeFilterCheckedListBox,
                "clearFiltersButton" => mainForm.clearFiltersButton,
                "showStatsButton" => mainForm.showStatsButton,
                "showCompletionButton" => mainForm.showCompletionButton,
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
    }
}