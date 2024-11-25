using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using ProcessMemory;
using Windows.Win32.Foundation;

namespace Assassin_s_Creed_Route_Tracker
{
    public partial class MainForm : Form
    {
        private ProcessMemoryHandler processMemoryHandler;
        private string currentProcess;
        private MultilevelPointer percentPtr;
        private MultilevelPointer ViewpointsPtr;
        private MultilevelPointer MyanPtr;
        private MultilevelPointer TreasurePtr;
        private MultilevelPointer FragmentsPtr;
        private MultilevelPointer WaterChestsPtr;
        private MultilevelPointer UnchartedChestsPtr;
        private MultilevelPointer AssassinPtr;
        private MultilevelPointer NavalPtr;
        private MultilevelPointer LettersPtr;
        private MultilevelPointer ManuscriptsPtr;
        private MultilevelPointer MusicPtr;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Assassin's Creed Route Tracker";
            this.BackColor = System.Drawing.Color.Black;
            this.ForeColor = System.Drawing.Color.White;

            Label connectionLabel = new Label();
            connectionLabel.Text = "Not connected";
            connectionLabel.Location = new System.Drawing.Point((this.ClientSize.Width - 100) / 2, 10);
            connectionLabel.AutoSize = true;
            this.Controls.Add(connectionLabel);

            ComboBox gameDropdown = new ComboBox();
            gameDropdown.Items.AddRange(new object[] { "", "Assassin's Creed 4", "Assassin's Creed Syndicate" });
            gameDropdown.SelectedIndex = 0;
            gameDropdown.Location = new System.Drawing.Point((this.ClientSize.Width - 800) / 2, 10);
            this.Controls.Add(gameDropdown);

            Button connectButton = new Button();
            connectButton.Text = "Connect to Game";
            connectButton.Location = new System.Drawing.Point((this.ClientSize.Width - 800) / 2, 40);
            connectButton.Click += ConnectButton_Click;
            this.Controls.Add(connectButton);

            Button percentageButton = new Button();
            percentageButton.Text = "Stats";
            percentageButton.Location = new System.Drawing.Point((this.ClientSize.Width - 100) / 2, 140);
            percentageButton.Click += PercentageButton_Click;
            this.Controls.Add(percentageButton);

            Label percentageLabel = new Label();
            percentageLabel.Text = "";
            percentageLabel.Location = new System.Drawing.Point((this.ClientSize.Width - 100) / 2, 180);
            percentageLabel.AutoSize = true;
            this.Controls.Add(percentageLabel);
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            ComboBox gameDropdown = (ComboBox)this.Controls[1];
            Label connectionLabel = (Label)this.Controls[0];
            string selectedGame = gameDropdown.SelectedItem.ToString();

            if (selectedGame == "Assassin's Creed 4")
                currentProcess = "AC4BFSP.exe";
            else if (selectedGame == "Assassin's Creed Syndicate")
                currentProcess = "ACS.exe";
            else
            {
                connectionLabel.Text = "Please select a game.";
                return;
            }

            Connect();
            if (processMemoryHandler != null)
                connectionLabel.Text = $"Connected to {selectedGame}";
            else
                connectionLabel.Text = "Error: Cannot connect to process. Make sure the game is running.";
        }

        private void PercentageButton_Click(object sender, EventArgs e)
        {
            Label percentageLabel = (Label)this.Controls[4];
            if (processMemoryHandler != null && currentProcess == "AC4BFSP.exe")
            {
                try
                {
                    //The pointers
                    int percent = percentPtr.DerefInt(0x284);
                    int Viewpoints = ViewpointsPtr.DerefInt(0x18);
                    int Myan = MyanPtr.DerefInt(0x18);
                    int Treasure = TreasurePtr.DerefInt(0xBF8);
                    int Fragments = FragmentsPtr.DerefInt(0x18);
                    int WaterChests = WaterChestsPtr.DerefInt(0x18);
                    int UnchartedChests = UnchartedChestsPtr.DerefInt(0x18);
                    int Assassin = AssassinPtr.DerefInt(0x778);
                    int Naval = NavalPtr.DerefInt(0x18);
                    int Letters = LettersPtr.DerefInt(0x678);
                    int Manuscripts = ManuscriptsPtr.DerefInt(0x18);
                    int Music = MusicPtr.DerefInt(0x18);

                    percentageLabel.Text = $"Completion Percentage: {percent}%\n" +
                        $"Viewpoints Completed: {Viewpoints}\n" +
                        $"Myan Stones Collected: {Myan}\n" +
                        $"Buried Treasure Collected: {Treasure}\n" +
                        $"AnimusFragments Collected: {Fragments}\n" +
                        $"WaterChests Collected: {WaterChests}\n" +
                        $"UnchatredChests Collected: {UnchartedChests}\n" +
                        $"AssassinContracts Completed: {Assassin}\n" +
                        $"NavalContracts Completed: {Naval}\n" +
                        $"LetterBottles Collected: {Letters}\n" +
                        $"Manuscripts Collected: {Manuscripts}\n" +
                        $"Music Sheets Collected: {Music}";

                }
                catch (Exception ex)
                {
                    percentageLabel.Text = $"Error: {ex.Message}";
                }
            }
            else if (processMemoryHandler != null && currentProcess == "ACS.exe")
                percentageLabel.Text = "Percentage feature not available for Assassin's Creed Syndicate";
            else
                percentageLabel.Text = "Not connected to a game";
        }

        private unsafe void Connect()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(currentProcess.Replace(".exe", ""));
                if (processes.Length > 0)
                {
                    Process process = processes[0];
                    processMemoryHandler = new ProcessMemoryHandler((uint)process.Id);

                    if (processMemoryHandler != null && currentProcess == "AC4BFSP.exe")
                    {
                        // Set up the percentage pointer for AC4
                        percentPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x49D9774));
                        ViewpointsPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x0002E8D0), 0x1A8, 0x28);
                        MyanPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x0002E8D0), 0x1A8, 0x3C);
                        TreasurePtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x01817920), 0x3AC);
                        FragmentsPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x0002E8D0), 0x1A8, 0x0);
                        WaterChestsPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x0002E8D0), 0x1A8, 0x64);
                        UnchartedChestsPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x0153A9DC), 0x158, 0x654);
                        AssassinPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x018FF260), 0x38C);
                        NavalPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x0002E8D0), 0x1A8, 0x168);
                        LettersPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x014218E8), 0x140);
                        ManuscriptsPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x0051D814), 0x6C, 0xA14);
                        MusicPtr = new MultilevelPointer(processMemoryHandler, (nint*)(process.MainModule?.BaseAddress + 0x016B6A7C), 0x54, 0x58C);
                    }
                }
                else
                {
                    processMemoryHandler = null;
                    percentPtr = null;
                }
            }
            catch (Exception)
            {
                processMemoryHandler = null;
                percentPtr = null;
            }
        }

    }
}
