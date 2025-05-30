using System;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Assassin_s_Creed_Route_Tracker
{
    [SupportedOSPlatform("windows6.1")]
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
