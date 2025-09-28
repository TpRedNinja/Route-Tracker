using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;


namespace Route_Tracker
{
    public static class UpdateManager
    {
        public static async Task CheckForUpdatesAsync()
        {
            if (Properties.Settings.Default.DevMode)
                return;

            if (!Properties.Settings.Default.CheckForUpdateOnStartup)
                return;

            string apiUrl = $"https://api.github.com/repos/{AppTheme.GitHubRepo}/releases";
            HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RouteTrackerUpdater");

            try
            {
                string response = await client.GetStringAsync(apiUrl);
                JsonDocument doc = JsonDocument.Parse(response);

                JsonElement? latestRelease = null;
                DateTime latestDate = DateTime.MinValue;

                foreach (JsonElement release in doc.RootElement.EnumerateArray())
                {
                    if (release.GetProperty("draft").GetBoolean())
                        continue;

                    if (release.TryGetProperty("published_at", out JsonElement publishedAtProp) &&
                        DateTime.TryParse(publishedAtProp.GetString(), out DateTime publishedAt))
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
                    Trace.WriteLine("Could not find any releases on GitHub.");
                    return;
                }

                string? latestVersion = latestRelease.Value.GetProperty("tag_name").GetString();
                if (string.IsNullOrEmpty(latestVersion))
                {
                    Trace.WriteLine("Could not determine the latest version from GitHub.");
                    return;
                }

                if (!string.Equals(latestVersion, AppTheme.Version, StringComparison.OrdinalIgnoreCase))
                {
                    await HandleUpdateFound(latestVersion, latestRelease.Value, client);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Update check failed: {ex.Message}");
            }
        }

        private static async Task HandleUpdateFound(string latestVersion, JsonElement release, HttpClient client)
        {
            DialogResult result = MessageBox.Show(
                $"A new version is available!\n\nCurrent: {AppTheme.Version}\nLatest: {latestVersion}\n\nDo you want to download and install it?",
                "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes) await DownloadAndInstallUpdate(release, client);
        }

        private static async Task DownloadAndInstallUpdate(JsonElement release, HttpClient client)
        {
            JsonElement assets = release.GetProperty("assets");
            if (assets.GetArrayLength() == 0)
            {
                Trace.WriteLine("No downloadable asset found in the latest release.");
                return;
            }

            // Prompt for download folder BEFORE showing the form
            string downloadFolder = "";
            using (FolderBrowserDialog fbd = new())
            {
                fbd.Description = "Select folder to download the update files to.";
                if (fbd.ShowDialog() != DialogResult.OK)
                    return;
                downloadFolder = fbd.SelectedPath;
            }

            UpdateProgressForm progressForm = new()
            {
                TopMost = true,
                LaunchNewVersionCheckBox = { Enabled = false },
                ContinueButton = { Enabled = false },
                StatusLabel = { Text = "Downloading update..." },
                DownloadProgressBar = { Value = 0 },
                ExtractProgressBar = { Value = 0 },
                DownloadPathBox = { Text = downloadFolder }
            };

            string? zipFilePath = null;
            string? zipFileNameNoExt = null;

            // Download all assets
            int assetCount = assets.GetArrayLength();
            int assetIndex = 0;
            bool downloadError = false;
            foreach (JsonElement asset in assets.EnumerateArray())
            {
                assetIndex++;
                string? assetName = asset.GetProperty("name").GetString();
                string? assetUrl = asset.GetProperty("browser_download_url").GetString();
                if (string.IsNullOrEmpty(assetName) || string.IsNullOrEmpty(assetUrl))
                    continue;

                string localPath = Path.Combine(downloadFolder, assetName);

                bool downloaded = false;
                while (!downloaded)
                {
                    try
                    {
                        progressForm.StatusLabel.Text = $"Downloading {assetName} ({assetIndex}/{assetCount})...";
                        progressForm.DownloadProgressBar.Value = (int)((assetIndex - 1) * 100.0 / assetCount);
                        Application.DoEvents();

                        using (HttpResponseMessage response = await client.GetAsync(assetUrl, HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode();
                            Stream assetStream = await response.Content.ReadAsStreamAsync();
                            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
                            FileStream fileStream = File.Create(localPath);
                            await assetStream.CopyToAsync(fileStream);
                        }
                        downloaded = true;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        MessageBox.Show($"Failed to download {assetName} because the folder does not exist.", "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        FolderBrowserDialog fbd = new()
                        {
                            Description = $"Select folder to save {assetName} to."
                        };
                        if (fbd.ShowDialog(progressForm) != DialogResult.OK)
                        {
                            downloadError = true;
                            break;
                        }
                        localPath = Path.Combine(fbd.SelectedPath, assetName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Download failed for {assetName}: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        downloadError = true;
                        break;
                    }
                }

                if (assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    zipFilePath = localPath;
                    zipFileNameNoExt = Path.GetFileNameWithoutExtension(assetName);
                }

                if (downloadError)
                    break;
            }

            progressForm.StatusLabel.Text = downloadError ? "Download failed." : "All downloads complete!";
            progressForm.DownloadProgressBar.Value = downloadError ? 0 : 100;
            Application.DoEvents();

            if (downloadError)
            {
                progressForm.LaunchNewVersionCheckBox.Enabled = true;
                progressForm.ContinueButton.Enabled = true;
                progressForm.ShowDialog();
                return;
            }

            // Prompt for extract folder after download
            string extractFolder = "";
            using (FolderBrowserDialog fbd = new() { Description = "Select folder to extract the update ZIP file to." })
            {
                if (fbd.ShowDialog(progressForm) != DialogResult.OK)
                {
                    progressForm.StatusLabel.Text = "Extraction cancelled.";
                    progressForm.LaunchNewVersionCheckBox.Enabled = true;
                    progressForm.ContinueButton.Enabled = true;
                    progressForm.ShowDialog();
                    return;
                }
                extractFolder = fbd.SelectedPath;
            }

            bool extractionError = false;
            if (!string.IsNullOrWhiteSpace(zipFilePath) && !string.IsNullOrWhiteSpace(extractFolder))
            {
                string extractTo = Path.Combine(extractFolder, zipFileNameNoExt ?? "ExtractedUpdate");
                try
                {
                    progressForm.StatusLabel.Text = "Extracting update...";
                    progressForm.ExtractProgressBar.Value = 0;
                    Application.DoEvents();

                    await Task.Run(() =>
                    {
                        ZipArchive archive = ZipFile.OpenRead(zipFilePath);
                        int totalEntries = archive.Entries.Count;
                        int currentEntry = 0;
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                string dirPath = Path.Combine(extractTo, entry.FullName);
                                Directory.CreateDirectory(dirPath);
                                continue;
                            }

                            string currentExtractTo = extractTo;
                            string destinationPath = Path.Combine(currentExtractTo, entry.FullName);
                            bool extracted = false;

                            while (!extracted)
                            {
                                try
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                                    entry.ExtractToFile(destinationPath, true);
                                    extracted = true;
                                }
                                catch (DirectoryNotFoundException)
                                {
                                    string? newFolder = null;
                                    progressForm.Invoke((Action)(() =>
                                    {
                                        MessageBox.Show(
                                            $"Failed to extract {entry.FullName} because the folder does not exist.",
                                            "Extraction Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                        FolderBrowserDialog fbd = new()
                                        {
                                            Description = $"Select folder to extract {entry.FullName} to."
                                        };
                                        if (fbd.ShowDialog(progressForm) == DialogResult.OK) newFolder = fbd.SelectedPath;
                                    }));

                                    if (!string.IsNullOrEmpty(newFolder))
                                    {
                                        currentExtractTo = newFolder;
                                        destinationPath = Path.Combine(currentExtractTo, entry.FullName);
                                    }
                                    else extracted = true;
                                }
                                catch (Exception ex)
                                {
                                    progressForm.Invoke((Action)(() =>
                                    {
                                        MessageBox.Show(
                                            $"Extraction failed for {entry.FullName}: {ex.Message}",
                                            "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }));
                                    extracted = true;
                                    extractionError = true;
                                }
                            }

                            currentEntry++;
                            int percent = (int)(currentEntry * 100 / totalEntries);
                            progressForm.Invoke(() =>
                            {
                                progressForm.ExtractProgressBar.Value = Math.Min(percent, 100);
                                progressForm.StatusLabel.Text = $"Extracting update... {percent}%";
                            });
                        }
                    });

                    progressForm.Invoke(() =>
                    {
                        progressForm.StatusLabel.Text = extractionError ? "Extraction failed." : "Update complete!";
                        progressForm.ExtractProgressBar.Value = extractionError ? 0 : 100;
                        progressForm.LaunchNewVersionCheckBox.Enabled = true;
                        progressForm.ContinueButton.Enabled = true;
                    });
                }
                catch (Exception ex)
                {
                    progressForm.Invoke(() =>
                    {
                        MessageBox.Show($"Extraction failed: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        progressForm.StatusLabel.Text = "Extraction failed.";
                        progressForm.LaunchNewVersionCheckBox.Enabled = true;
                        progressForm.ContinueButton.Enabled = true;
                    });
                    extractionError = true;
                }
            }
            else
            {
                progressForm.Invoke(() =>
                {
                    progressForm.StatusLabel.Text = "No ZIP file extracted.";
                    progressForm.LaunchNewVersionCheckBox.Enabled = true;
                    progressForm.ContinueButton.Enabled = true;
                });
                extractionError = true;
            }

            progressForm.ShowDialog();

            bool launchNewVersion = progressForm.LaunchNewVersionCheckBox.Checked;

            if (!extractionError && launchNewVersion && !string.IsNullOrWhiteSpace(extractFolder) && !string.IsNullOrWhiteSpace(zipFileNameNoExt))
            {
                string newExePath = Path.Combine(extractFolder, zipFileNameNoExt, "Route Tracker.exe");
                if (File.Exists(newExePath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(newExePath) { UseShellExecute = true });
                        Application.Exit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to launch new version: {ex.Message}", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else MessageBox.Show("New executable not found. Please launch manually.", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

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
    }
}