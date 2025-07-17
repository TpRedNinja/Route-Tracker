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
            this.Size = new Size(500, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.BackgroundColor;
            this.ForeColor = AppTheme.TextColor;

            // Scrollable panel for help content
            var scrollPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(480, 390),
                AutoScroll = true,
                BackColor = AppTheme.BackgroundColor
            };

            infoLabel = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(460, 0),
                ForeColor = AppTheme.TextColor,
                BackColor = AppTheme.BackgroundColor,
                Font = new Font(FontFamily.GenericSansSerif, 11),
                Location = new Point(0, 0)
            };
            scrollPanel.Controls.Add(infoLabel);

            prevButton = new Button
            {
                Text = "Previous",
                Size = new Size(90, 30),
                Location = new Point(60, 420)
            };
            prevButton.Click += (s, e) => { step--; UpdateStep(); };

            nextButton = new Button
            {
                Text = "Next",
                Size = new Size(90, 30),
                Location = new Point(200, 420)
            };
            nextButton.Click += (s, e) => { step++; UpdateStep(); };

            closeButton = new Button
            {
                Text = "Close",
                Size = new Size(90, 30),
                Location = new Point(340, 420)
            };
            closeButton.Click += (s, e) => this.Close();

            AppTheme.ApplyToButton(prevButton);
            AppTheme.ApplyToButton(nextButton);
            AppTheme.ApplyToButton(closeButton);

            this.Controls.Add(scrollPanel);
            this.Controls.Add(prevButton);
            this.Controls.Add(nextButton);
            this.Controls.Add(closeButton);

            LoadHelpPages();
            UpdateStep();
        }

        // Helper to auto-adjust font size if needed
        private void AdjustFontToFit(string text)
        {
            int minFont = 8;
            int maxFont = 11;
            Font font = new(FontFamily.GenericSansSerif, maxFont);
            infoLabel.Font = font;
            infoLabel.Text = text;
            infoLabel.MaximumSize = new Size(460, 0);
            infoLabel.AutoSize = true;

            // If label is too tall for panel, shrink font
            while (infoLabel.Height > 390 && font.Size > minFont)
            {
                font = new Font(FontFamily.GenericSansSerif, font.Size - 1);
                infoLabel.Font = font;
                infoLabel.Text = text;
                infoLabel.MaximumSize = new Size(460, 0);
                infoLabel.AutoSize = true;
            }
        }

        private void UpdateStep()
        {
            if (helpPages.Count == 0)
            {
                AdjustFontToFit("No help content available.");
                prevButton.Enabled = false;
                nextButton.Enabled = false;
                return;
            }

            if (step < 0) step = 0;
            if (step >= helpPages.Count) step = helpPages.Count - 1;

            prevButton.Enabled = step > 0;
            nextButton.Enabled = step < helpPages.Count - 1;

            var page = helpPages[step];

            var shortcuts = parentForm.settingsManager != null
            ? parentForm.settingsManager.GetShortcuts()
            : (Load: Keys.None, Save: Keys.None, LoadProgress: Keys.None,
               ResetProgress: Keys.None, Refresh: Keys.None, Help: Keys.None,
               FilterClear: Keys.None, Connect: Keys.None, GameStats: Keys.None,
               RouteStats: Keys.None, LayoutUp: Keys.None, LayoutDown: Keys.None,
               BackupFolder: Keys.None, BackupNow: Keys.None, Restore: Keys.None,
               SetFolder: Keys.None, AutoTog: Keys.None, TopTog: Keys.None,
               AdvTog: Keys.None, GlobalTog: Keys.None, SortingUp: Keys.None,
               SortingDown: Keys.None, GameDirect: Keys.None);

            var keysConverter = new KeysConverter();

            string content = page.Content
            .Replace("{Load}", keysConverter.ConvertToString(shortcuts.Load))
            .Replace("{Save}", keysConverter.ConvertToString(shortcuts.Save))
            .Replace("{LoadProgress}", keysConverter.ConvertToString(shortcuts.LoadProgress))
            .Replace("{ResetProgress}", keysConverter.ConvertToString(shortcuts.ResetProgress))
            .Replace("{Refresh}", keysConverter.ConvertToString(shortcuts.Refresh))
            .Replace("{Help}", keysConverter.ConvertToString(shortcuts.Help))
            .Replace("{FilterClear}", keysConverter.ConvertToString(shortcuts.FilterClear))
            .Replace("{Connect}", keysConverter.ConvertToString(shortcuts.Connect))
            .Replace("{GameStats}", keysConverter.ConvertToString(shortcuts.GameStats))
            .Replace("{RouteStats}", keysConverter.ConvertToString(shortcuts.RouteStats))
            .Replace("{LayoutUp}", keysConverter.ConvertToString(shortcuts.LayoutUp))
            .Replace("{LayoutDown}", keysConverter.ConvertToString(shortcuts.LayoutDown))
            .Replace("{BackupFolder}", keysConverter.ConvertToString(shortcuts.BackupFolder))
            .Replace("{BackupNow}", keysConverter.ConvertToString(shortcuts.BackupNow))
            .Replace("{Restore}", keysConverter.ConvertToString(shortcuts.Restore))
            .Replace("{SetFolder}", keysConverter.ConvertToString(shortcuts.SetFolder))
            .Replace("{AutoTog}", keysConverter.ConvertToString(shortcuts.AutoTog))
            .Replace("{TopTog}", keysConverter.ConvertToString(shortcuts.TopTog))
            .Replace("{AdvTog}", keysConverter.ConvertToString(shortcuts.AdvTog))
            .Replace("{GlobalTog}", keysConverter.ConvertToString(shortcuts.GlobalTog))
            .Replace("{SortingUp}", keysConverter.ConvertToString(shortcuts.SortingUp))
            .Replace("{SortingDown}", keysConverter.ConvertToString(shortcuts.SortingDown))
            .Replace("{GameDirect}", keysConverter.ConvertToString(shortcuts.GameDirect));

            AdjustFontToFit($"{page.Title}\n\n{content}");
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
    }
}