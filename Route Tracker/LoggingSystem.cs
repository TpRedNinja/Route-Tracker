using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Comprehensive logging system for error tracking and developer feedback
    // Handles local file logging, email delivery, and user consent management
    // Provides easy-to-use methods for replacing existing error handling
    // ==========MY NOTES==============
    // This is the main logging system that handles everything from writing logs
    // to sending them via email when the app closes
    public class LoggingSystem
    {
        #region Constants and Configuration
        private static readonly string LogsFolder;
        private static readonly string CurrentLogFile;
        private static readonly string LastVersionFile;
        
        // Email configuration
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "TpRouteTrackerApp@gmail.com";
        private const string SenderPassword = "cjut ymzr citu xvij";
        private const string PrimaryRecipient = "topredninja@gmail.com";
        private const string FallbackRecipient = "hjdomangue@outlook.com";
        
        // Developer device detection
        private static readonly string[] DeveloperDeviceNames = ["TpRedNinja"];

        // Testing flags - REMOVE THESE FOR PRODUCTION
        private static readonly bool IsTestMode = Environment.GetEnvironmentVariable("ROUTE_TRACKER_TEST_MODE") == "true";
        private static readonly bool ForceEmailSend = Environment.GetEnvironmentVariable("ROUTE_TRACKER_FORCE_EMAIL") == "true";
        
        static LoggingSystem()
        {
            // Use the same folder as SearchHistoryManager for consistency
            LogsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RouteTracker",
                "Route Tracker json files"
            );
            
            Directory.CreateDirectory(LogsFolder);
            
            CurrentLogFile = Path.Combine(LogsFolder, $"RouteTracker_v{AppTheme.Version}_Log.txt");
            LastVersionFile = Path.Combine(LogsFolder, "LastVersion.txt");
            
            InitializeLogFile();
        }
        #endregion

        #region Log File Management
        // ==========MY NOTES==============
        // Sets up the log file for the current session
        // Creates new file if version changed, adds date separator if needed
        private static void InitializeLogFile()
        {
            try
            {
                bool isNewVersion = CheckForVersionChange();
                bool needsDateSeparator = CheckForDateChange();

                if (isNewVersion)
                {
                    // Create new log file for new version
                    if (File.Exists(CurrentLogFile))
                    {
                        string backupFile = Path.Combine(LogsFolder, 
                            $"RouteTracker_v{GetLastVersion()}_Log_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                        File.Move(CurrentLogFile, backupFile);
                    }
                }
                else if (needsDateSeparator && File.Exists(CurrentLogFile))
                {
                    // Add date separator for new day
                    File.AppendAllText(CurrentLogFile, $"\n--------\n");
                }

                // Log session start
                string sessionStart = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] === Route Tracker {AppTheme.Version} Session Started ===\n";
                File.AppendAllText(CurrentLogFile, sessionStart);
                
                // Update version tracking
                File.WriteAllText(LastVersionFile, AppTheme.Version);
            }
            catch (Exception ex)
            {
                // Fallback: write to console if logging fails
                System.Diagnostics.Debug.WriteLine($"Failed to initialize log file: {ex.Message}");
            }
        }

        // ==========MY NOTES==============
        // Checks if the app version has changed since last run
        private static bool CheckForVersionChange()
        {
            if (!File.Exists(LastVersionFile))
                return true;

            try
            {
                string lastVersion = File.ReadAllText(LastVersionFile).Trim();
                return lastVersion != AppTheme.Version;
            }
            catch
            {
                return true;
            }
        }

        // ==========MY NOTES==============
        // Gets the last known version from file
        private static string GetLastVersion()
        {
            try
            {
                if (File.Exists(LastVersionFile))
                    return File.ReadAllText(LastVersionFile).Trim();
            }
            catch { }
            return "Unknown";
        }

        // ==========MY NOTES==============
        // Checks if we need to add a date separator
        private static bool CheckForDateChange()
        {
            if (!File.Exists(CurrentLogFile))
                return false;

            try
            {
                var fileInfo = new FileInfo(CurrentLogFile);
                return fileInfo.LastWriteTime.Date != DateTime.Now.Date;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Public Logging Methods
        // ==========MY NOTES==============
        // Main logging method for errors - replaces Debug.WriteLine calls
        public static void LogError(string message, Exception? exception = null)
        {
            LogMessage("ERROR", message, exception);
        }

        // ==========MY NOTES==============
        // Logs important events and information
        public static void LogInfo(string message)
        {
            LogMessage("INFO", message);
        }

        // ==========MY NOTES==============
        // Logs warnings that aren't errors but are noteworthy
        public static void LogWarning(string message)
        {
            LogMessage("WARNING", message);
        }

        // ==========MY NOTES==============
        // Core logging method that writes to the log file
        private static void LogMessage(string level, string message, Exception? exception = null)
        {
            try
            {
                var logEntry = new StringBuilder();
                logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {message}");
                
                if (exception != null)
                {
                    logEntry.AppendLine($"Exception Type: {exception.GetType().Name}");
                    logEntry.AppendLine($"Exception Message: {exception.Message}");
                    logEntry.AppendLine($"Stack Trace: {exception.StackTrace}");
                    if (exception.InnerException != null)
                    {
                        logEntry.AppendLine($"Inner Exception: {exception.InnerException.Message}");
                    }
                }
                
                File.AppendAllText(CurrentLogFile, logEntry.ToString());
            }
            catch
            {
                // If logging fails, there's nothing we can do
            }
        }
        #endregion

        #region Developer Device Detection
        // ==========MY NOTES==============
        // Checks if running on developer's machine to skip email sending
        private static bool IsRunningOnDeveloperDevice()
        {
            try
            {
                string currentDeviceName = Environment.MachineName;
                
                foreach (string devName in DeveloperDeviceNames)
                {
                    if (string.Equals(currentDeviceName, devName, StringComparison.OrdinalIgnoreCase))
                    {
                        LogInfo($"Developer device detected: {currentDeviceName}");
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogError("Failed to detect device name", ex);
                return false; // Assume not developer device if detection fails
            }
        }
        #endregion

        #region Email Functionality
        // ==========MY NOTES==============
        // Main method called when app closes - prompts user and sends log if consented
        public static async Task HandleApplicationExit()
        {
            try
            {
                // Skip email sending if on developer device (unless forced for testing)
                if (IsRunningOnDeveloperDevice() && !ForceEmailSend)
                {
                    LogInfo("Skipping email prompt - running on developer device");
                    return;
                }

                // Check if there's anything worth sending
                if (!HasLogContent())
                {
                    LogInfo("No significant log content - skipping email prompt");
                    return;
                }

                // In test mode, log but don't actually send emails
                if (IsTestMode)
                {
                    LogInfo("TEST MODE: Would prompt user for email consent here");
                    return;
                }

                // Prompt user for consent
                bool userConsented = ShowEmailConsentDialog();
                if (!userConsented)
                {
                    LogInfo("User declined to send log file");
                    return;
                }

                // Get optional user comment
                string userComment = GetUserComment();
                
                // Attempt to send email
                bool emailSent = await TrySendLogEmail(userComment);
                
                if (emailSent)
                {
                    // Ask if user wants to clear the log file
                    bool clearLog = ShowClearLogDialog();
                    if (clearLog)
                    {
                        ClearLogFile();
                    }
                }
                else
                {
                    // Show manual contact information
                    ShowManualContactDialog();
                }
            }
            catch (Exception ex)
            {
                LogError("Failed during application exit handling", ex);
            }
        }

        // ==========MY NOTES==============
        // Checks if the log file has content worth sending
        private static bool HasLogContent()
        {
            try
            {
                if (!File.Exists(CurrentLogFile))
                    return false;

                string content = File.ReadAllText(CurrentLogFile);
                
                // Check for errors or warnings (not just session starts)
                return content.Contains("ERROR:") || content.Contains("WARNING:") || 
                       content.Length > 500; // Or if log is substantial
            }
            catch
            {
                return false;
            }
        }

        // ==========MY NOTES==============
        // Shows privacy-aware consent dialog for email sending
        private static bool ShowEmailConsentDialog()
        {
            var result = MessageBox.Show(
                "Route Tracker would like to send error logs to the developer to help improve the application.\n\n" +
                "The log file contains:\n" +
                "• Application errors and diagnostic information\n" +
                "• Timestamps of when events occurred\n" +
                "• No personal information or game data\n\n" +
                "This data will only be used for debugging and improving the application.\n\n" +
                "Do you consent to sending this log file?",
                "Send Error Log to Developer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        // ==========MY NOTES==============
        // Gets optional comment from user
        private static string GetUserComment()
        {
            using var commentForm = new LogCommentForm();
            if (commentForm.ShowDialog() == DialogResult.OK)
            {
                return commentForm.UserComment;
            }
            return string.Empty;
        }

        // ==========MY NOTES==============
        // Attempts to send the log file via email with fallback options
        private static async Task<bool> TrySendLogEmail(string userComment)
        {
            try
            {
                // Try primary email first
                bool primarySuccess = await SendEmailTo(PrimaryRecipient, userComment);
                if (primarySuccess)
                {
                    LogInfo("Log successfully sent to primary email");
                    return true;
                }

                LogWarning("Primary email failed, trying fallback");

                // Try fallback email
                bool fallbackSuccess = await SendEmailTo(FallbackRecipient, userComment);
                if (fallbackSuccess)
                {
                    LogInfo("Log successfully sent to fallback email");
                    return true;
                }

                LogWarning("Fallback email failed, trying user email option");

                // Try user's email as last resort
                return await TryUserEmailOption(userComment);
            }
            catch (Exception ex)
            {
                LogError("Failed to send log email", ex);
                return false;
            }
        }

        // ==========MY NOTES==============
        // Sends email to specific recipient
        private static async Task<bool> SendEmailTo(string recipient, string userComment)
        {
            try
            {
                using var client = new SmtpClient(SmtpServer, SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(SenderEmail, SenderPassword)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(SenderEmail, "Route Tracker App"),
                    Subject = $"Route Tracker {AppTheme.Version} Error Log - {DateTime.Now:yyyy-MM-dd}",
                    Body = CreateEmailBody(userComment),
                    IsBodyHtml = false
                };

                message.To.Add(recipient);

                // Attach log file
                if (File.Exists(CurrentLogFile))
                {
                    message.Attachments.Add(new Attachment(CurrentLogFile));
                }

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to send email to {recipient}", ex);
                return false;
            }
        }

        // ==========MY NOTES==============
        // Creates the email body with user comment and system info
        private static string CreateEmailBody(string userComment)
        {
            var body = new StringBuilder();
            body.AppendLine($"Route Tracker {AppTheme.Version} Error Log");
            body.AppendLine($"Sent on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            body.AppendLine($"Device: {Environment.MachineName}");
            body.AppendLine($"OS: {Environment.OSVersion}");
            body.AppendLine($".NET Version: {Environment.Version}");
            body.AppendLine();

            if (!string.IsNullOrWhiteSpace(userComment))
            {
                body.AppendLine("User Comment:");
                body.AppendLine(userComment);
                body.AppendLine();
            }

            body.AppendLine("Please find the detailed log file attached.");
            body.AppendLine();
            body.AppendLine("This email was sent automatically with user consent from Route Tracker.");

            return body.ToString();
        }

        // ==========MY NOTES==============
        // Handles user email option as last resort
        private static Task<bool> TryUserEmailOption(string userComment)
        {
            var result = MessageBox.Show(
                "Both developer email addresses failed to receive the log.\n\n" +
                "As a last option, would you like to try sending from your own email address?\n" +
                "(This will open your default email client)",
                "Try User Email",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                bool success = TryOpenEmailClient(userComment);
                return Task.FromResult(success);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        // ==========MY NOTES==============
        // Opens user's email client with pre-filled message
        private static bool TryOpenEmailClient(string userComment)
        {
            try
            {
                string subject = Uri.EscapeDataString($"Route Tracker {AppTheme.Version} Error Log");
                string body = Uri.EscapeDataString($"Please find attached log file.\n\nUser Comment:\n{userComment}");
                string mailto = $"mailto:{PrimaryRecipient}?subject={subject}&body={body}";

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = mailto,
                    UseShellExecute = true
                });

                MessageBox.Show(
                    $"Please attach the log file from:\n{CurrentLogFile}\n\nThen send the email.",
                    "Attach Log File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                LogError("Failed to open email client", ex);
                return false;
            }
        }

        // ==========MY NOTES==============
        // Shows dialog asking if user wants to clear log after successful send
        private static bool ShowClearLogDialog()
        {
            var result = MessageBox.Show(
                "Log file sent successfully!\n\n" +
                "Would you like to clear the log file to start fresh?",
                "Clear Log File",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        // ==========MY NOTES==============
        // Clears the current log file
        private static void ClearLogFile()
        {
            try
            {
                File.WriteAllText(CurrentLogFile, 
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Log file cleared after successful email send\n");
                LogInfo("Log file cleared at user request");
            }
            catch (Exception ex)
            {
                LogError("Failed to clear log file", ex);
            }
        }

        // ==========MY NOTES==============
        // Shows manual contact information when email fails
        private static void ShowManualContactDialog()
        {
            MessageBox.Show(
                "Unable to send the log file automatically.\n\n" +
                "Please send the log file manually to 'TpRedNinja' on Discord.\n" +
                "Send a friend request first, then try to send the log file.\n\n" +
                $"Log file location:\n{CurrentLogFile}",
                "Manual Contact Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        #endregion

        #region Manual Send Option
        // ==========MY NOTES==============
        // Manual send option for UI - just shows contact info
        public static void ShowManualSendOption()
        {
            ShowManualContactDialog();
        }
        #endregion

        #region Testing Helpers
        // ==========MY NOTES==============
        // TESTING ONLY: Simulates various log events for testing
        public static void SimulateTestEvents()
        {
            LogInfo("=== TESTING: Simulating various log events ===");
            LogInfo("This is a test info message");
            LogWarning("This is a test warning message");
            LogError("This is a test error message", new Exception("Test exception for logging"));
            LogInfo("=== TESTING: Simulation complete ===");
        }
        #endregion
    }
}