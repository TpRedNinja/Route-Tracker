using System;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Route_Tracker
{
    [SupportedOSPlatform("windows6.1")]
    public partial class ConnectionWindow : Form
    {
        private readonly GameConnectionManager gameConnectionManager;
        private readonly SettingsManager settingsManager;
        private Label? connectionLabel;
        private ComboBox? gameDropdown;
        private Button? autoDetectButton;
        private Button? connectButton;

        public string SelectedGame => gameDropdown!.SelectedItem?.ToString() ?? string.Empty;

        public ConnectionWindow(GameConnectionManager gameConnectionManager, SettingsManager settingsManager)
        {
            this.gameConnectionManager = gameConnectionManager;
            this.settingsManager = settingsManager;
            InitializeComponent();
            InitializeCustomComponents();
            this.TopMost = true;
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Connect to Game";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 400;
            this.Height = 180;

            connectionLabel = new Label
            {
                Text = "Not connected",
                AutoSize = true,
                Top = 20,
                Left = 20
            };
            this.Controls.Add(connectionLabel);

            gameDropdown = new ComboBox
            {
                Left = 20,
                Top = 50,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gameDropdown.Items.AddRange(["", "Assassin's Creed 4", "God of War 2018"]);
            gameDropdown.SelectedIndex = 0;
            this.Controls.Add(gameDropdown);

            autoDetectButton = new Button
            {
                Text = "Auto-Detect",
                Left = 230,
                Top = 50,
                Width = 100
            };
            autoDetectButton.Click += AutoDetectButton_Click;
            this.Controls.Add(autoDetectButton);

            connectButton = new Button
            {
                Text = "Connect",
                Left = 20,
                Top = 90,
                Width = 310
            };
            connectButton.Click += ConnectButton_Click;
            this.Controls.Add(connectButton);

        }

        private void AutoDetectButton_Click(object? sender, EventArgs e)
        {
            string detectedGame = gameConnectionManager.DetectRunningGame();
            if (!string.IsNullOrEmpty(detectedGame))
            {
                gameDropdown!.SelectedItem = detectedGame;
                connectionLabel!.Text = $"Detected: {detectedGame}";
            }
            else
            {
                connectionLabel!.Text = "No supported games detected";
            }
        }

        private async void ConnectButton_Click(object? sender, EventArgs e)
        {
            string selectedGame = gameDropdown!.SelectedItem?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(selectedGame))
            {
                connectionLabel!.Text = "Please select a game.";
                return;
            }

            string gameDirectory = settingsManager.GetGameDirectory(selectedGame);
            if (string.IsNullOrEmpty(gameDirectory))
            {
                connectionLabel!.Text = "Game directory not set.";
                return;
            }

            bool connected = await gameConnectionManager.ConnectToGameAsync(selectedGame, false);
            if (connected)
            {
                connectionLabel!.Text = $"Connected to {selectedGame}";
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                connectionLabel!.Text = "Error: Cannot connect to process. Make sure the game is running.";
            }
        }
    }
}