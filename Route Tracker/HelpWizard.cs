using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace Route_Tracker
{
    public partial class HelpWizard : Form
    {
        private int step = 0;
        private readonly List<HelpPage> helpPages = [];
        private readonly Label infoLabel;
        private readonly Button nextButton;
        private readonly Button prevButton;
        private readonly Button closeButton;
        private readonly HotkeysSettingsForm parentForm;

        private class HelpPage
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = "";
            [JsonPropertyName("content")]
            public string Content { get; set; } = "";
        }

        public HelpWizard(HotkeysSettingsForm parentForm)
        {
            this.parentForm = parentForm;
            this.Text = "Help";
            this.Size = new Size(500, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.BackgroundColor;
            this.ForeColor = AppTheme.TextColor;

            infoLabel = new Label
            {
                AutoSize = false,
                Location = new Point(20, 20),
                Size = new Size(440, 180),
                ForeColor = AppTheme.TextColor,
                BackColor = AppTheme.BackgroundColor,
                Font = new Font(FontFamily.GenericSansSerif, 11),
            };

            prevButton = new Button
            {
                Text = "Previous",
                Size = new Size(90, 30),
                Location = new Point(120, 220)
            };
            prevButton.Click += (s, e) => { step--; UpdateStep(); };

            nextButton = new Button
            {
                Text = "Next",
                Size = new Size(90, 30),
                Location = new Point(220, 220)
            };
            nextButton.Click += (s, e) => { step++; UpdateStep(); };

            closeButton = new Button
            {
                Text = "Close",
                Size = new Size(90, 30),
                Location = new Point(320, 220)
            };
            closeButton.Click += (s, e) => this.Close();

            AppTheme.ApplyToButton(prevButton);
            AppTheme.ApplyToButton(nextButton);
            AppTheme.ApplyToButton(closeButton);

            this.Controls.Add(infoLabel);
            this.Controls.Add(prevButton);
            this.Controls.Add(nextButton);
            this.Controls.Add(closeButton);

            LoadHelpPages();
            UpdateStep();
        }

        private void LoadHelpPages()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpPages.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var pages = JsonSerializer.Deserialize<List<HelpPage>>(json);
                if (pages != null)
                    helpPages.AddRange(pages);
            }
        }

        private void UpdateStep()
        {
            if (helpPages.Count == 0)
            {
                infoLabel.Text = "No help content available.";
                prevButton.Enabled = false;
                nextButton.Enabled = false;
                return;
            }

            if (step < 0) step = 0;
            if (step >= helpPages.Count) step = helpPages.Count - 1;

            prevButton.Enabled = step > 0;
            nextButton.Enabled = step < helpPages.Count - 1;

            var page = helpPages[step];
            var shortcuts = parentForm.settingsManager?.GetShortcuts()
                ?? (Load: Keys.None, Save: Keys.None, LoadProgress: Keys.None, Refresh: Keys.None, Help: Keys.None, FilterClear: Keys.None);

            var keysConverter = new KeysConverter();

            // Debug: Show raw content and shortcut values
            Debug.WriteLine($"DEBUG: Page Title: {page.Title}");
            Debug.WriteLine($"DEBUG: Raw Content: {page.Content}");
            Debug.WriteLine($"DEBUG: Shortcuts: Load={shortcuts.Load}, Save={shortcuts.Save}, LoadProgress={shortcuts.LoadProgress}, Refresh={shortcuts.Refresh}, Help={shortcuts.Help}, FilterClear={shortcuts.FilterClear}");

            string content = page.Content
                .Replace("{Load}", keysConverter.ConvertToString(shortcuts.Load))
                .Replace("{Save}", keysConverter.ConvertToString(shortcuts.Save))
                .Replace("{LoadProgress}", keysConverter.ConvertToString(shortcuts.LoadProgress))
                .Replace("{Refresh}", keysConverter.ConvertToString(shortcuts.Refresh))
                .Replace("{Help}", keysConverter.ConvertToString(shortcuts.Help))
                .Replace("{FilterClear}", keysConverter.ConvertToString(shortcuts.FilterClear));

            // Debug: Show replaced content
            Debug.WriteLine($"DEBUG: Replaced Content: {content}");

            infoLabel.Text = $"{page.Title}\n\n{content}";
        }
    }
}