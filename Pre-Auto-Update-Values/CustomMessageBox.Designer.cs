namespace Assassin_s_Creed_Route_Tracker
{
    partial class CustomMessageBox
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label messageLabel;
        private System.Windows.Forms.Button tryAgainButton;
        private System.Windows.Forms.Button waitButton;
        private System.Windows.Forms.Button manuallyButton;
        private System.Windows.Forms.Button cancelButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.messageLabel = new System.Windows.Forms.Label();
            this.tryAgainButton = new System.Windows.Forms.Button();
            this.waitButton = new System.Windows.Forms.Button();
            this.manuallyButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // messageLabel
            // 
            this.messageLabel.AutoSize = true;
            this.messageLabel.Location = new System.Drawing.Point(12, 9);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(50, 20);
            this.messageLabel.TabIndex = 0;
            this.messageLabel.Text = "label1";
            // 
            // tryAgainButton
            // 
            this.tryAgainButton.Location = new System.Drawing.Point(16, 50);
            this.tryAgainButton.Name = "tryAgainButton";
            this.tryAgainButton.Size = new System.Drawing.Size(94, 29);
            this.tryAgainButton.TabIndex = 1;
            this.tryAgainButton.Text = "Try Again";
            this.tryAgainButton.UseVisualStyleBackColor = true;
            this.tryAgainButton.Click += new System.EventHandler(this.TryAgainButton_Click);
            // 
            // waitButton
            // 
            this.waitButton.Location = new System.Drawing.Point(116, 50);
            this.waitButton.Name = "waitButton";
            this.waitButton.Size = new System.Drawing.Size(94, 29);
            this.waitButton.TabIndex = 2;
            this.waitButton.Text = "Wait 10 sec's";
            this.waitButton.UseVisualStyleBackColor = true;
            this.waitButton.Click += new System.EventHandler(this.WaitButton_Click);
            // 
            // manuallyButton
            // 
            this.manuallyButton.Location = new System.Drawing.Point(216, 50);
            this.manuallyButton.Name = "manuallyButton";
            this.manuallyButton.Size = new System.Drawing.Size(94, 29);
            this.manuallyButton.TabIndex = 3;
            this.manuallyButton.Text = "Manually";
            this.manuallyButton.UseVisualStyleBackColor = true;
            this.manuallyButton.Click += new System.EventHandler(this.ManuallyButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(316, 50);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(94, 29);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // CustomMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(422, 91);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.manuallyButton);
            this.Controls.Add(this.waitButton);
            this.Controls.Add(this.tryAgainButton);
            this.Controls.Add(this.messageLabel);
            this.Name = "CustomMessageBox";
            this.Text = "CustomMessageBox";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
