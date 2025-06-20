using System;
using System.Windows.Forms;

namespace Assassin_s_Creed_Route_Tracker
{
    public partial class CustomMessageBox : Form
    {
        public enum CustomDialogResult
        {
            TryAgain,
            Wait,
            Manually,
            Cancel
        }

        public CustomDialogResult Result { get; private set; }

        public CustomMessageBox(string message)
        {
            InitializeComponent();
            messageLabel.Text = message;
        }

        private void tryAgainButton_Click(object sender, EventArgs e)
        {
            Result = CustomDialogResult.TryAgain;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void waitButton_Click(object sender, EventArgs e)
        {
            Result = CustomDialogResult.Wait;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void manuallyButton_Click(object sender, EventArgs e)
        {
            Result = CustomDialogResult.Manually;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = CustomDialogResult.Cancel;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
