using System;
using System.Windows.Forms;

namespace Assassin_s_Creed_Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Custom dialog that provides multiple options when waiting for a game to start
    // Extends the standard Form class with specialized result types and UI
    // ==========MY NOTES==============
    // This is a custom popup window that shows options when the game is taking too long to start
    // It gives choices like try again, wait longer, do it manually, or cancel
    public partial class CustomMessageBox : Form
    {
        // ==========FORMAL COMMENT=========
        // Enumeration of possible dialog results for the custom message box
        // Defines the specific action selected by the user
        // ==========MY NOTES==============
        // These are the different choices the user can make in the dialog
        public enum CustomDialogResult
        {
            TryAgain,
            Wait,
            Manually,
            Cancel
        }

        // ==========FORMAL COMMENT=========
        // Stores the selected action chosen by the user
        // Property is read-only externally but set by button click handlers
        // ==========MY NOTES==============
        // This tracks which button the user clicked
        public CustomDialogResult Result { get; private set; }

        // ==========FORMAL COMMENT=========
        // Initializes a new custom message box with the specified message
        // Sets up the UI elements and displays the provided message
        // ==========MY NOTES==============
        // Creates the dialog and shows the message passed in
        public CustomMessageBox(string message)
        {
            InitializeComponent();
            messageLabel.Text = message;
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Try Again button clicks
        // Sets the dialog result and closes the form
        // ==========MY NOTES==============
        // Runs when the user clicks "Try Again"
        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            Result = CustomDialogResult.TryAgain;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Wait button clicks
        // Sets the dialog result and closes the form
        // ==========MY NOTES==============
        // Runs when the user clicks "Wait Longer"
        private void WaitButton_Click(object sender, EventArgs e)
        {
            Result = CustomDialogResult.Wait;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Manually button clicks
        // Sets the dialog result and closes the form
        // ==========MY NOTES==============
        // Runs when the user clicks "Start Manually"
        private void ManuallyButton_Click(object sender, EventArgs e)
        {
            Result = CustomDialogResult.Manually;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ==========FORMAL COMMENT=========
        // Event handler for Cancel button clicks
        // Sets the dialog result and closes the form
        // ==========MY NOTES==============
        // Runs when the user clicks "Cancel"
        private void CancelButton_Click(object sender, EventArgs e)
        {
            Result = CustomDialogResult.Cancel;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}