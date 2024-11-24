using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ProcessMemory;

namespace Assassin_s_Creed_Route_Tracker
{
    public partial class MainForm : Form
    {
        private ProcessMemoryHandler processMemoryHandler;
        private string currentProcess;
        private MultilevelPointer percentagePointer;

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
            if (processMemoryHandler != null && currentProcess == "AC4BFSP.exe" && percentagePointer != null)
            {
                try
                {
                    int percentage = percentagePointer.DerefInt(0x284);
                    percentageLabel.Text = $"Completion Percentage: {percentage}%";
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
                        percentagePointer = new MultilevelPointer(processMemoryHandler,
                            (nint*)process.MainModule?.BaseAddress + 0x049D9774);
                    }
                }
                else
                {
                    processMemoryHandler = null;
                    percentagePointer = null;
                }
            }
            catch (Exception)
            {
                processMemoryHandler = null;
                percentagePointer = null;
            }
        }
    }
}
