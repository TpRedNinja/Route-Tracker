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
            this.FormBorderStyle = FormBorderStyle.Sizable; // Allows resizing and shows all control buttons
            this.MaximizeBox = true;    // Show maximize button
            this.MinimizeBox = true;    // Show minimize button
            this.ControlBox = true;     // Show control box (X, -, square)
            this.BackColor = Color.Black;
            this.TopMost = true;

            // Panel for scrollable content
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Black
            };

            // Stats label
            statsLabel = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Segoe UI", 14f),
                ForeColor = Color.White,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(10),
                MaximumSize = new Size(460, 0)
            };
            contentPanel.Controls.Add(statsLabel);

            // REMOVE the custom close button code!

            this.Controls.Add(contentPanel);
        }

        // Updates the stats display with the provided text.
        public void UpdateStats(string statsText)
        {
            statsLabel.Text = statsText;
        }
    }
}