using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // This makes everything look consistent with the dark theme
    // Keeps all styling in one place so it's easy to change the look later
    public static class AppTheme
    {
        #region App Information (from AppTheme.cs)
        public const string Version = "v1.01";
        public const string GitHubRepo = "TpRedNinja/Route-Tracker";
        #endregion

        // Colors
        public static readonly Color BackgroundColor = Color.Black;
        public static readonly Color InputBackgroundColor = Color.Black;
        public static readonly Color TextColor = Color.White;
        public static readonly Color AccentColor = Color.DarkRed;
        public static readonly Color TransparentKey = Color.FromArgb(50, 50, 50); // Chroma key color for OBS

        // Enhanced dark colors for better Windows 11 integration
        public static readonly Color MenuBackgroundColor = Color.Black;
        public static readonly Color MenuHoverColor = Color.Black;
        public static readonly Color MenuBorderColor = Color.Black;
        public static readonly Color ScrollBarColor = Color.FromArgb(45, 45, 45);

        // Fonts
        public static readonly Font DefaultFont = new("Segoe UI", 9f);
        public static readonly Font HeaderFont = new("Segoe UI", 12f, FontStyle.Bold);
        public static readonly Font StatsFont = new("Segoe UI", 14f);

        // Spacing
        public const int StandardPadding = 10;
        public const int StandardMargin = 5;

        // ==========MY NOTES==============
        // One method to make any control match our dark theme
        public static void ApplyTo(Control control)
        {
            control.BackColor = BackgroundColor;
            control.ForeColor = TextColor;
            control.Font = DefaultFont;
        }

        // ==========MY NOTES==============
        // Enhanced theme application for menus and strips
        public static void ApplyToMenuStrip(MenuStrip menuStrip)
        {
            menuStrip.BackColor = MenuBackgroundColor;
            menuStrip.ForeColor = TextColor;
            menuStrip.Renderer = new DarkMenuRenderer();
        }

        // ==========MY NOTES==============
        // Enhanced theme application for context menus
        public static void ApplyToContextMenu(ContextMenuStrip contextMenu)
        {
            contextMenu.BackColor = MenuBackgroundColor;
            contextMenu.ForeColor = TextColor;
            contextMenu.Renderer = new DarkMenuRenderer();
        }

        // ==========MY NOTES==============
        // Apply enhanced styling to buttons
        public static void ApplyToButton(Button button)
        {
            button.BackColor = MenuBackgroundColor;
            button.ForeColor = TextColor;
            button.Font = DefaultFont;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = MenuBorderColor;
            button.FlatAppearance.MouseOverBackColor = MenuHoverColor;
            button.FlatAppearance.MouseDownBackColor = MenuBackgroundColor;
        }

        // ==========MY NOTES==============
        // Apply enhanced styling to text boxes
        public static void ApplyToTextBox(TextBox textBox)
        {
            textBox.BackColor = MenuBackgroundColor;
            textBox.ForeColor = TextColor;
            textBox.Font = DefaultFont;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        // ==========MY NOTES==============
        // Apply enhanced styling to combo boxes
        public static void ApplyToComboBox(ComboBox comboBox)
        {
            comboBox.BackColor = MenuBackgroundColor;
            comboBox.ForeColor = TextColor;
            comboBox.Font = DefaultFont;
            comboBox.FlatStyle = FlatStyle.Flat;
        }

        // ==========MY NOTES==============
        // Apply dark theme to settings forms with proper white text
        public static void ApplyToSettingsForm(Form form)
        {
            form.BackColor = BackgroundColor;
            form.ForeColor = TextColor;

            // Apply theme to all controls on the form
            foreach (Control control in form.Controls)
            {
                ApplyTo(control);
            }
        }
    }

    // ==========MY NOTES==============
    // Helper to safely get values from dictionaries without crashes
    public static class DictionaryExtensions
    {
        public static T GetValueOrDefault<T>(this Dictionary<string, object> dict, string key, T defaultValue)
        {
            if (dict.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }
    }

    // ==========MY NOTES==============
    // Custom renderer for dark mode menus without Windows API
    public class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColorTable()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                using var brush = new SolidBrush(AppTheme.MenuHoverColor);
                e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
            }
            else
            {
                using var brush = new SolidBrush(AppTheme.MenuBackgroundColor);
                e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = AppTheme.TextColor; // Force white text
            base.OnRenderItemText(e);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using var pen = new Pen(AppTheme.MenuBorderColor);
            var rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            e.Graphics.DrawRectangle(pen, rect);
        }
    }

    // ==========MY NOTES==============
    // Custom color table for enhanced dark theming
    public class DarkColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => AppTheme.MenuHoverColor;
        public override Color MenuItemSelectedGradientBegin => AppTheme.MenuHoverColor;
        public override Color MenuItemSelectedGradientEnd => AppTheme.MenuHoverColor;
        public override Color MenuItemPressedGradientBegin => AppTheme.AccentColor;
        public override Color MenuItemPressedGradientEnd => AppTheme.AccentColor;
        public override Color MenuItemBorder => AppTheme.MenuBorderColor;
        public override Color MenuBorder => AppTheme.MenuBorderColor;
        public override Color ToolStripDropDownBackground => AppTheme.MenuBackgroundColor;
        public override Color ImageMarginGradientBegin => AppTheme.MenuBackgroundColor;
        public override Color ImageMarginGradientEnd => AppTheme.MenuBackgroundColor;
        public override Color ImageMarginGradientMiddle => AppTheme.MenuBackgroundColor;
        public override Color SeparatorDark => AppTheme.MenuBorderColor;
        public override Color SeparatorLight => AppTheme.MenuBorderColor;
        public override Color ToolStripBorder => AppTheme.MenuBorderColor;
        public override Color ToolStripGradientBegin => AppTheme.MenuBackgroundColor;
        public override Color ToolStripGradientEnd => AppTheme.MenuBackgroundColor;
        public override Color ToolStripGradientMiddle => AppTheme.MenuBackgroundColor;
    }
}