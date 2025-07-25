using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Route_Tracker
{
    // ==========MY NOTES==============
    // Loading spinner window that shows during route loading and sorting operations
    // Transparent overlay that prevents user interaction during processing
    public partial class LoadingSpinner : Form
    {
        private AppTimer animationTimer = null!;
        private float rotationAngle = 0f;
        private readonly Form parentForm;
        private Font? loadingFont;
        private Brush? loadingBrush;

        public LoadingSpinner(Form parent)
        {
            parentForm = parent;
            InitializeLoadingSpinner();
            SetupAnimation();
            CreateDrawingResources();
        }

        private void CreateDrawingResources()
        {
            // Create drawing resources once to avoid IUIService issues
            loadingFont = new Font("Segoe UI", 12, FontStyle.Regular);
            loadingBrush = new SolidBrush(Color.White);
        }

        private void InitializeLoadingSpinner()
        {
            // Make it a transparent overlay
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.Opacity = 0.3;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Make it cover the parent window exactly
            this.Size = parentForm.Size;
            this.Location = parentForm.Location;

            // Follow parent window movements
            parentForm.LocationChanged += (s, e) => this.Location = parentForm.Location;
            parentForm.SizeChanged += (s, e) => this.Size = parentForm.Size;

            // Enable double buffering for smooth animation
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.DoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
        }

        private void SetupAnimation()
        {
            // Create a UI timer for smooth animation
            animationTimer = AppTimer.CreateUITimer(50, (s, e) =>
            {
                if (this.Visible)
                {
                    rotationAngle += 15f; // Rotate 15 degrees per frame
                    if (rotationAngle >= 360f)
                        rotationAngle = 0f;
                    this.Invalidate(); // Trigger repaint
                }
            });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (loadingFont == null || loadingBrush == null)
                return;

            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Calculate center point
            int centerX = this.Width / 2;
            int centerY = this.Height / 2;

            // Draw spinning circle
            int radius = 30;
            using (Pen pen = new Pen(Color.White, 4))
            {
                // Save the current transform
                Matrix? originalTransform = g.Transform.Clone();

                // Translate to center, rotate, then translate back
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(rotationAngle);
                g.TranslateTransform(-centerX, -centerY);

                // Draw the arc (3/4 circle for spinner effect)
                Rectangle rect = new Rectangle(centerX - radius, centerY - radius, radius * 2, radius * 2);
                g.DrawArc(pen, rect, 0, 270);

                // Restore the original transform
                g.Transform = originalTransform;
                originalTransform?.Dispose();
            }

            // Draw loading text using pre-created resources
            string text = "Loading...";
            SizeF textSize = g.MeasureString(text, loadingFont);
            float textX = (this.Width - textSize.Width) / 2;
            float textY = centerY + radius + 20;
            g.DrawString(text, loadingFont, loadingBrush, textX, textY);
        }

        public void ShowSpinner()
        {
            this.Show();
            animationTimer.Start();
            this.BringToFront();
            Application.DoEvents(); // Ensure it shows immediately
        }

        public void HideSpinner()
        {
            animationTimer.Stop();
            this.Hide();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer?.Dispose();
                loadingFont?.Dispose();
                loadingBrush?.Dispose();
            }
            base.Dispose(disposing);
        }

        // Prevent the form from being moved or interacted with
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTTRANSPARENT = -1;

            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTTRANSPARENT;
                return;
            }

            base.WndProc(ref m);
        }
    }
}