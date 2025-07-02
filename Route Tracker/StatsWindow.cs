using System;
using System.Windows.Forms;
using System.Drawing;

namespace Route_Tracker
{
    public partial class StatsWindow : Form
    {
        private readonly Label statsLabel;
        private readonly Panel contentPanel;

        public StatsWindow()
        {
            InitializeComponent();
            this.Text = "Game Statistics";
            this.Width = 500;
            this.Height = 600;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.ControlBox = true;
            this.TopMost = true;

            // Apply AppTheme
            AppTheme.ApplyTo(this);

            // Panel for scrollable content
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppTheme.BackgroundColor
            };

            // Stats label
            statsLabel = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = AppTheme.StatsFont,
                ForeColor = AppTheme.TextColor,
                BackColor = AppTheme.BackgroundColor,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(AppTheme.StandardPadding),
                MaximumSize = new Size(460, 0)
            };

            contentPanel.Controls.Add(statsLabel);
            this.Controls.Add(contentPanel);
        }

        public void UpdateStats(string statsText)
        {
            statsLabel.Text = statsText;
        }
    }
}