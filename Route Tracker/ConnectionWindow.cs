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
            this.SetupAsSettingsForm("Connect to Game");
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 400;
            this.Height = 180;

            connectionLabel = UIControlFactory.CreateThemedLabel("Not connected");
            connectionLabel.Location = new Point(20, 20);
            this.Controls.Add(connectionLabel);

            gameDropdown = new ComboBox
            {
                Left = 20,
                Top = 50,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gameDropdown.Items.Add("");
            gameDropdown.Items.AddRange([.. SupportedGames.GameList.Values.Select(g => g.DisplayName)]);
            gameDropdown.SelectedIndex = 0;
            this.Controls.Add(gameDropdown);

            autoDetectButton = UIControlFactory.CreateThemedButton("Auto-Detect", 100, 25);
            autoDetectButton.Location = new Point(230, 50);
            autoDetectButton.Click += AutoDetectButton_Click;
            this.Controls.Add(autoDetectButton);

            connectButton = UIControlFactory.CreateThemedButton("Connect", 310, 25);
            connectButton.Location = new Point(20, 90);
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