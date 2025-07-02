using System.Windows.Forms;
using System.Drawing;

namespace Route_Tracker
{
    public partial class DevPasscodeForm : Form
    {
        private readonly TextBox passcodeTextBox;
        private readonly Button okButton;
        private readonly Button cancelButton;

        public string Passcode => passcodeTextBox.Text;

        public DevPasscodeForm()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Size = new Size(320, 140);
            this.Text = "Developer Passcode";
            this.TopMost = true;

            AppTheme.ApplyToSettingsForm(this);

            var label = new Label
            {
                Text = "Enter developer passcode:",
                AutoSize = true,
                Location = new Point(12, 15)
            };
            this.Controls.Add(label);

            passcodeTextBox = new TextBox
            {
                Location = new Point(15, 40),
                Width = 270,
                UseSystemPasswordChar = true
            };
            this.Controls.Add(passcodeTextBox);

            okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(120, 75),
                Width = 75
            };
            this.Controls.Add(okButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(210, 75),
                Width = 75
            };
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            // Apply theme to buttons and other stuff
            AppTheme.ApplyToButton(okButton);
            AppTheme.ApplyToButton(cancelButton);
            AppTheme.ApplyToTextBox(passcodeTextBox);
        }
    }
}