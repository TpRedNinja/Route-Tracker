using System;
using System.Drawing;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Simple form for collecting optional user comments when sending logs
    // Provides a text area for users to describe what they were doing when the error occurred
    // ==========MY NOTES==============
    // Small dialog that lets users add comments to the log email
    public partial class LogCommentForm : Form
    {
        private TextBox commentTextBox = null!;
        private Button sendButton = null!;
        private Button skipButton = null!;

        public string UserComment { get; private set; } = string.Empty;

        public LogCommentForm()
        {
            InitializeCustomComponents();
            AppTheme.ApplyToSettingsForm(this);
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Add Comment (Optional)";
            this.Size = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Instructions label
            var instructionLabel = new Label
            {
                Text = "You can add an optional comment describing what you were doing when the error occurred:",
                Location = new Point(10, 10),
                Size = new Size(360, 40),
                Font = AppTheme.DefaultFont
            };
            this.Controls.Add(instructionLabel);

            // Comment text box
            commentTextBox = new TextBox
            {
                Location = new Point(10, 60),
                Size = new Size(360, 150),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                PlaceholderText = "Optional: Describe what you were doing, what game you were connected to, etc.",
                Font = AppTheme.DefaultFont
            };
            this.Controls.Add(commentTextBox);

            // Button panel
            var buttonPanel = new Panel
            {
                Location = new Point(10, 220),
                Size = new Size(360, 35),
                Dock = DockStyle.Bottom
            };

            sendButton = new Button
            {
                Text = "Send with Comment",
                Location = new Point(180, 5),
                Size = new Size(120, 25),
                DialogResult = DialogResult.OK
            };
            sendButton.Click += (s, e) =>
            {
                UserComment = commentTextBox.Text.Trim();
                this.Close();
            };

            skipButton = new Button
            {
                Text = "Send without Comment",
                Location = new Point(50, 5),
                Size = new Size(120, 25),
                DialogResult = DialogResult.OK
            };
            skipButton.Click += (s, e) =>
            {
                UserComment = string.Empty;
                this.Close();
            };

            buttonPanel.Controls.Add(skipButton);
            buttonPanel.Controls.Add(sendButton);
            this.Controls.Add(buttonPanel);

            AppTheme.ApplyToButton(sendButton);
            AppTheme.ApplyToButton(skipButton);
            AppTheme.ApplyToTextBox(commentTextBox);
        }
    }
}