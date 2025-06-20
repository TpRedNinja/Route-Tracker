using System;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Main program class that serves as the application entry point
    // Initializes the Windows Forms environment and launches the main form
    // ==========MY NOTES==============
    // This is where the app starts - it just sets up Windows Forms and shows the main window
    [SupportedOSPlatform("windows6.1")]
    static class Program
    {
        // ==========FORMAL COMMENT=========
        // Application entry point that configures the UI environment and runs the main form
        // Sets up visual styles and creates the primary application window
        // ==========MY NOTES==============
        // The starting point for the whole app
        // Does some Windows Forms setup and then creates our main window
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
