using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // Helper class to easily show/hide loading spinner during operations
    public static class LoadingHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "Because i said so")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "Because i said so")]
        public static async Task ExecuteWithSpinner(Form parentForm, Func<Task> operation, string loadingText = "Loading...")
        {
            LoadingSpinner? spinner = null;
            try
            {
                // Show spinner
                spinner = new LoadingSpinner(parentForm);
                spinner.ShowSpinner();

                // Execute the operation
                await operation();
            }
            finally
            {
                // Always hide spinner
                spinner?.HideSpinner();
                spinner?.Dispose();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "Because i said so")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "Because i said so")]
        public static async Task<T> ExecuteWithSpinner<T>(Form parentForm, Func<Task<T>> operation, string loadingText = "Loading...")
        {
            LoadingSpinner? spinner = null;
            try
            {
                // Show spinner
                spinner = new LoadingSpinner(parentForm);
                spinner.ShowSpinner();

                // Execute the operation and return result
                return await operation();
            }
            finally
            {
                // Always hide spinner
                spinner?.HideSpinner();
                spinner?.Dispose();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "Because i said so")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "Because i said so")]
        public static async Task ExecuteWithSpinnerAsync(Form parentForm, Action operation, string loadingText = "Loading...")
        {
            LoadingSpinner? spinner = null;
            try
            {
                // Show spinner
                spinner = new LoadingSpinner(parentForm);
                spinner.ShowSpinner();

                // Execute the operation on a background thread to prevent freezing
                await Task.Run(operation);
            }
            finally
            {
                // Always hide spinner on UI thread
                if (parentForm.InvokeRequired)
                {
                    parentForm.Invoke(() =>
                    {
                        spinner?.HideSpinner();
                        spinner?.Dispose();
                    });
                }
                else
                {
                    spinner?.HideSpinner();
                    spinner?.Dispose();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "Because i said so")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "Because i said so")]
        public static void ExecuteWithSpinner(Form parentForm, Action operation, string loadingText = "Loading...")
        {
            LoadingSpinner? spinner = null;
            try
            {
                // Show spinner
                spinner = new LoadingSpinner(parentForm);
                spinner.ShowSpinner();

                // Execute the operation
                operation();
            }
            finally
            {
                // Always hide spinner
                spinner?.HideSpinner();
                spinner?.Dispose();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060",
        Justification = "Because i said so")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "Because i said so")]
        public static T ExecuteWithSpinner<T>(Form parentForm, Func<T> operation, string loadingText = "Loading...")
        {
            LoadingSpinner? spinner = null;
            try
            {
                // Show spinner
                spinner = new LoadingSpinner(parentForm);
                spinner.ShowSpinner();

                // Execute the operation and return result
                return operation();
            }
            finally
            {
                // Always hide spinner
                spinner?.HideSpinner();
                spinner?.Dispose();
            }
        }
    }
}