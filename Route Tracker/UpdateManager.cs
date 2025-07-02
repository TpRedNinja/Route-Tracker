using System;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Route_Tracker
{
    public static class UpdateManager
    {
        // ==========MY NOTES==============
        // Checks GitHub for updates and downloads them automatically if user agrees
        public static async Task CheckForUpdatesAsync()
        {
            if (Properties.Settings.Default.DevMode)
                return;

            if (!Properties.Settings.Default.CheckForUpdateOnStartup)
                return;

            string apiUrl = $"https://api.github.com/repos/{AppTheme.GitHubRepo}/releases";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RouteTrackerUpdater");

            try
            {
                var response = await client.GetStringAsync(apiUrl);
                using var doc = JsonDocument.Parse(response);

                JsonElement? latestRelease = null;
                DateTime latestDate = DateTime.MinValue;

                foreach (var release in doc.RootElement.EnumerateArray())
                {
                    if (release.GetProperty("draft").GetBoolean())
                        continue;

                    if (release.TryGetProperty("published_at", out var publishedAtProp) &&
                        DateTime.TryParse(publishedAtProp.GetString(), out var publishedAt))
                    {
                        if (publishedAt > latestDate)
                        {
                            latestDate = publishedAt;
                            latestRelease = release;
                        }
                    }
                }

                if (latestRelease == null)
                {
                    MessageBox.Show("Could not find any releases on GitHub.", "Update Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string? latestVersion = latestRelease.Value.GetProperty("tag_name").GetString();
                if (string.IsNullOrEmpty(latestVersion))
                {
                    MessageBox.Show("Could not determine the latest version from GitHub.", "Update Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!string.Equals(latestVersion, AppTheme.Version, StringComparison.OrdinalIgnoreCase))
                {
                    await HandleUpdateFound(latestVersion, latestRelease.Value, client);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check failed: {ex.Message}");
            }
        }

        // ==========MY NOTES==============
        // Shows update dialog and handles user response
        private static async Task HandleUpdateFound(string latestVersion, JsonElement release, HttpClient client)
        {
            var result = MessageBox.Show(
                $"A new version is available!\n\nCurrent: {AppTheme.Version}\nLatest: {latestVersion}\n\nDo you want to download and install it?",
                "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                await DownloadAndInstallUpdate(release, client);
            }
        }

        // ==========MY NOTES==============
        // Downloads and installs the update from GitHub
        private static async Task DownloadAndInstallUpdate(JsonElement release, HttpClient client)
        {
            var assets = release.GetProperty("assets");
            if (assets.GetArrayLength() > 0)
            {
                string? zipUrl = assets[0].GetProperty("browser_download_url").GetString();
                if (string.IsNullOrEmpty(zipUrl))
                {
                    MessageBox.Show("No download URL found for the latest release.", "Update Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string tempZip = Path.GetTempFileName();
                using (var zipStream = await client.GetStreamAsync(zipUrl))
                using (var fileStream = File.Create(tempZip))
                    await zipStream.CopyToAsync(fileStream);

                ZipFile.ExtractToDirectory(tempZip, AppDomain.CurrentDomain.BaseDirectory, true);

                MessageBox.Show("Update complete. Please restart the application.", "Update Complete", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No downloadable asset found in the latest release.", "Update Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========MY NOTES==============
        // Adds the update check setting to the settings menu
        public static void AddUpdateCheckMenuItem(ToolStripMenuItem settingsMenuItem)
        {
            ToolStripMenuItem checkForUpdatesMenuItem = new("Check for Updates on Startup")
            {
                CheckOnClick = true,
                Checked = Properties.Settings.Default.CheckForUpdateOnStartup
            };
            checkForUpdatesMenuItem.CheckedChanged += (s, e) =>
            {
                Properties.Settings.Default.CheckForUpdateOnStartup = checkForUpdatesMenuItem.Checked;
                Properties.Settings.Default.Save();
            };
            settingsMenuItem.DropDownItems.Add(checkForUpdatesMenuItem);
        }

        // ==========MY NOTES==============
        // Adds the dev mode setting with passcode protection
        public static void AddDevModeMenuItem(ToolStripMenuItem settingsMenuItem)
        {
            var devModeMenuItem = new ToolStripMenuItem("Enable Dev Mode")
            {
                CheckOnClick = true,
                Checked = Properties.Settings.Default.DevMode
            };
            devModeMenuItem.CheckedChanged += (s, e) =>
            {
                if (devModeMenuItem.Checked)
                {
                    using var passForm = new DevPasscodeForm();
                    if (passForm.ShowDialog() == DialogResult.OK)
                    {
                        if (passForm.Passcode == "1289")
                        {
                            Properties.Settings.Default.DevMode = true;
                            Properties.Settings.Default.Save();
                            MessageBox.Show("Dev Mode enabled. Update checks will be skipped.", "Dev Mode", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            devModeMenuItem.Checked = false;
                            MessageBox.Show("Incorrect passcode.", "Dev Mode", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        devModeMenuItem.Checked = false;
                    }
                }
                else
                {
                    Properties.Settings.Default.DevMode = false;
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Dev Mode disabled.", "Dev Mode", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            settingsMenuItem.DropDownItems.Add(devModeMenuItem);
        }
    }
}