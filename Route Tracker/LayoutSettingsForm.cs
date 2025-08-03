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
            this.SetupAsSettingsForm("Layout Settings");
            this.Size = new Size(350, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Apply theme
            //AppTheme.ApplyTo(this);

            // Layout selection
            var layoutLabel = UIControlFactory.CreateThemedLabel("Layout Mode:");
            layoutLabel.Location = new Point(20, 20);
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
            var (okButton, cancelButton) = UIControlFactory.CreateOkCancelButtons();
            okButton.Location = new Point(155, 70);
            okButton.Click += OkButton_Click;

            cancelButton.Location = new Point(235, 70);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);

            // Store references
            this.okButton = okButton;
            this.cancelButton = cancelButton;
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