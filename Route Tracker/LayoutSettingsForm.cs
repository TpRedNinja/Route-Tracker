using System;
using System.Windows.Forms;

namespace Route_Tracker
{
    public partial class LayoutSettingsForm : Form
    {
        public enum LayoutMode
        {
            Normal,
            Compact,
            Mini,
            Overlay
        }

        public LayoutMode SelectedLayout { get; private set; }
        private ComboBox layoutComboBox = null!;
        private Button okButton = null!;
        private Button cancelButton = null!;

        public LayoutSettingsForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Layout Settings";
            this.Size = new Size(350, 150); // Make it smaller since no transparency option
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Apply theme
            AppTheme.ApplyTo(this);

            // Layout selection
            var layoutLabel = new Label
            {
                Text = "Layout Mode:",
                Location = new Point(20, 20),
                Size = new Size(100, 23),
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont
            };
            this.Controls.Add(layoutLabel);

            layoutComboBox = new ComboBox
            {
                Location = new Point(130, 20),
                Size = new Size(180, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = AppTheme.InputBackgroundColor,
                ForeColor = AppTheme.TextColor,
                Font = AppTheme.DefaultFont,
                FlatStyle = FlatStyle.Flat
            };

            layoutComboBox.Items.AddRange([
            "Normal - Full interface",
            "Compact - Hide filters & buttons",
            "Mini - Just completion & next item",
            "Overlay - Minimal for streaming"
        ]);

            layoutComboBox.SelectedIndex = 0;
            this.Controls.Add(layoutComboBox);

            // Buttons
            okButton = new Button
            {
                Text = "OK",
                Location = new Point(155, 70), // Move up since no transparency option
                Size = new Size(75, 25),
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;
            AppTheme.ApplyTo(okButton);
            this.Controls.Add(okButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(235, 70), // Move up since no transparency option
                Size = new Size(75, 25),
                DialogResult = DialogResult.Cancel
            };
            AppTheme.ApplyTo(cancelButton);
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            SelectedLayout = (LayoutMode)layoutComboBox.SelectedIndex;
        }

        public void LoadCurrentSettings(LayoutMode currentLayout)
        {
            layoutComboBox.SelectedIndex = (int)currentLayout;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}