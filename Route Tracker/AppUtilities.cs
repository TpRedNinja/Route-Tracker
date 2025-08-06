using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // This makes everything look consistent with the dark theme
    // Keeps all styling in one place so it's easy to change the look later
    public static class AppTheme
    {
        #region App Information (from AppTheme.cs)
        public const string Version = "v1.04";
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
        // Standard form sizes for consistency
        public static class FormSizes
        {
            public static readonly Size Small = new(350, 200);
            public static readonly Size Medium = new(500, 400);
            public static readonly Size Large = new(700, 600);
            public static readonly Size Settings = new(400, 300);
        }

        // ==========MY NOTES==============
        // One method to make any control match our dark theme
        public static void ApplyTo(Control control)
        {
            control.BackColor = BackgroundColor;
            control.ForeColor = TextColor;
            control.Font = DefaultFont;
        }

        // ==========MY NOTES==============
        // Apply theme to multiple controls at once
        public static void ApplyToControls(params Control[] controls)
        {
            foreach (var control in controls)
            {
                switch (control)
                {
                    case Button btn: ApplyToButton(btn); break;
                    case TextBox txt: ApplyToTextBox(txt); break;
                    case ComboBox cmb: ApplyToComboBox(cmb); break;
                    default: ApplyTo(control); break;
                }
            }
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
    // Factory methods for creating themed UI controls with consistent styling
    public static class UIControlFactory
    {
        public static Button CreateThemedButton(string text, int width = 100, int height = 25)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(width, height),
                Font = AppTheme.DefaultFont
            };
            AppTheme.ApplyToButton(button);
            return button;
        }

        public static Label CreateThemedLabel(string text, bool autoSize = true)
        {
            return new Label
            {
                Text = text,
                AutoSize = autoSize,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont
            };
        }

        public static TextBox CreateThemedTextBox(int width = 200, string placeholder = "")
        {
            var textBox = new TextBox
            {
                Width = width,
                PlaceholderText = placeholder,
                Font = AppTheme.DefaultFont
            };
            AppTheme.ApplyToTextBox(textBox);
            return textBox;
        }

        public static ComboBox CreateThemedComboBox(int width = 200)
        {
            var comboBox = new ComboBox
            {
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTheme.DefaultFont
            };
            AppTheme.ApplyToComboBox(comboBox);
            return comboBox;
        }

        public static Form CreateThemedForm(string title, int width = 400, int height = 300)
        {
            var form = new Form
            {
                Text = title,
                Size = new Size(width, height),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };
            AppTheme.ApplyTo(form);
            return form;
        }

        public static (Button okButton, Button cancelButton) CreateOkCancelButtons()
        {
            var okButton = CreateThemedButton("OK", 75, 25);
            var cancelButton = CreateThemedButton("Cancel", 75, 25);

            okButton.DialogResult = DialogResult.OK;
            cancelButton.DialogResult = DialogResult.Cancel;

            return (okButton, cancelButton);
        }
    }

    // ==========MY NOTES==============
    // Extension methods for common form layout operations
    public static class FormExtensions
    {
        public static void AddButtonRow(this Form form, params Button[] buttons)
        {
            int buttonY = form.Height - 60;
            int spacing = 10;
            int totalWidth = buttons.Sum(b => b.Width) + (spacing * (buttons.Length - 1));
            int startX = (form.Width - totalWidth) / 2;

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Location = new Point(startX, buttonY);
                startX += buttons[i].Width + spacing;
                form.Controls.Add(buttons[i]);
            }
        }

        public static void AddLabeledControl(this Form form, string labelText, Control control, int y)
        {
            var label = UIControlFactory.CreateThemedLabel(labelText);
            label.Location = new Point(20, y);

            control.Location = new Point(130, y - 2);

            form.Controls.Add(label);
            form.Controls.Add(control);
        }

        public static void SetupAsSettingsForm(this Form form, string title)
        {
            form.Text = title;
            form.Size = AppTheme.FormSizes.Settings;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.StartPosition = FormStartPosition.CenterParent;
            AppTheme.ApplyTo(form);
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