using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Handles downloading route files from URLs
    // Manages HTTP requests, progress reporting, and file validation
    // Structured for easy expansion to bulk download functionality
    // ==========MY NOTES==============
    // Core download logic separated from UI for reusability
    // Can be extended later for bulk downloads or other download types
    public class RouteDownloadManager
    {
        private readonly HttpClient httpClient;
        private readonly RouteHistoryManager historyManager;

        public RouteDownloadManager()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Route Tracker");
            historyManager = new RouteHistoryManager();
        }

        public async Task<string> DownloadRouteAsync(string url, string filename, IProgress<int>? progress = null)
        {
            // Validate URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Invalid URL format.");
            }

            // Validate filename
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Filename cannot be empty.");
            }

            if (!filename.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".tsv";
            }

            // Get download path
            string downloadPath = historyManager.GetDownloadPath(filename);
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(downloadPath) ?? throw new InvalidOperationException("Invalid download path"));

            try
            {
                // Start download
                using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                // Check content type (optional validation)
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType != null && !IsValidContentType(contentType))
                {
                    // Log warning but don't fail - some servers don't set correct content type
                    System.Diagnostics.Debug.WriteLine($"Warning: Content type '{contentType}' may not be a text file");
                }

                // Get total length for progress reporting
                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var totalBytesRead = 0L;

                // Download with progress reporting
                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write);

                var buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalBytesRead += bytesRead;

                    // Report progress
                    if (progress != null && totalBytes > 0)
                    {
                        var progressPercentage = (int)((totalBytesRead * 100) / totalBytes);
                        progress.Report(progressPercentage);
                    }
                }

                // Validate downloaded file
                await ValidateDownloadedFile(downloadPath);

                return downloadPath;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to download from URL: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Clean up partial file
                if (File.Exists(downloadPath))
                {
                    try { File.Delete(downloadPath); } catch { }
                }
                throw new InvalidOperationException($"Download failed: {ex.Message}", ex);
            }
        }

        private static bool IsValidContentType(string contentType)
        {
            return contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
                   contentType.Contains("plain", StringComparison.OrdinalIgnoreCase) ||
                   contentType.Contains("tab-separated", StringComparison.OrdinalIgnoreCase) ||
                   contentType.Contains("tsv", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task ValidateDownloadedFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Downloaded file not found.");
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
            {
                throw new InvalidOperationException("Downloaded file is empty.");
            }

            // Basic validation - check if file contains TSV-like content
            try
            {
                using var reader = new StreamReader(filePath);
                var firstLine = await reader.ReadLineAsync();
                
                if (string.IsNullOrWhiteSpace(firstLine))
                {
                    throw new InvalidOperationException("Downloaded file appears to be empty or invalid.");
                }

                // Check if it looks like TSV (contains tabs)
                if (!firstLine.Contains('\t'))
                {
                    System.Diagnostics.Debug.WriteLine("Warning: Downloaded file may not be in TSV format (no tabs found in first line)");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Downloaded file validation failed: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}