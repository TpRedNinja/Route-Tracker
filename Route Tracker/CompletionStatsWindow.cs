﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace Route_Tracker
{
    public class CompletionStatsWindow : Form
    {
        private readonly Label percentageLabel;
        private readonly Label fractionLabel;
        private readonly Label titleLabel;

        public CompletionStatsWindow()
        {
            // Form setup
            this.Text = "Route Completion Stats";
            this.Size = new Size(300, 180);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Apply AppTheme
            AppTheme.ApplyTo(this);

            // Title label
            titleLabel = new Label
            {
                Text = "Route Completion Statistics",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = AppTheme.HeaderFont,
                ForeColor = AppTheme.TextColor,
                BackColor = AppTheme.BackgroundColor,
                Height = 30,
                Margin = new Padding(0, AppTheme.StandardPadding, 0, AppTheme.StandardPadding)
            };

            // Percentage label
            percentageLabel = new Label
            {
                Text = "Completion: 0.00%",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = AppTheme.StatsFont,
                ForeColor = AppTheme.TextColor, // Use accent color for main stat
                BackColor = AppTheme.BackgroundColor,
                Height = 40,
                Margin = new Padding(0, AppTheme.StandardPadding, 0, 0)
            };

            // Fraction label
            fractionLabel = new Label
            {
                Text = "Completed: 0/0 entries",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = AppTheme.DefaultFont,
                ForeColor = AppTheme.TextColor,
                BackColor = AppTheme.BackgroundColor,
                Height = 30
            };

            // Add controls in reverse order for proper stacking
            this.Controls.Add(fractionLabel);
            this.Controls.Add(percentageLabel);
            this.Controls.Add(titleLabel);
        }

        public void UpdateStats(float percentage, int completed, int total)
        {
            percentageLabel.Text = $"Completion: {percentage:F2}%";
            fractionLabel.Text = $"Completed: {completed}/{total} entries";
        }
    }
}