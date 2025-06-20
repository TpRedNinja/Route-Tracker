namespace Assassin_s_Creed_Route_Tracker
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

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
            autoStartCheckBox = new CheckBox();
            gameDirectoryTextBox = new TextBox();
            browseButton = new Button();
            saveButton = new Button();
            SuspendLayout();
            // 
            // autoStartCheckBox
            // 
            autoStartCheckBox.AutoSize = true;
            autoStartCheckBox.Location = new Point(12, 12);
            autoStartCheckBox.Name = "autoStartCheckBox";
            autoStartCheckBox.Size = new Size(194, 34);
            autoStartCheckBox.TabIndex = 0;
            autoStartCheckBox.Text = "Auto-Start Game";
            autoStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // gameDirectoryTextBox
            // 
            gameDirectoryTextBox.Location = new Point(2, 52);
            gameDirectoryTextBox.Name = "gameDirectoryTextBox";
            gameDirectoryTextBox.ReadOnly = true;
            gameDirectoryTextBox.Size = new Size(260, 35);
            gameDirectoryTextBox.TabIndex = 1;
            // 
            // browseButton
            // 
            browseButton.Location = new Point(278, 37);
            browseButton.Name = "browseButton";
            browseButton.Size = new Size(113, 50);
            browseButton.TabIndex = 2;
            browseButton.Text = "Browse";
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += BrowseButton_Click;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(2, 110);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(75, 43);
            saveButton.TabIndex = 3;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButton_Click;
            // 
            // SettingsForm
            // 
            ClientSize = new Size(400, 203);
            Controls.Add(saveButton);
            Controls.Add(browseButton);
            Controls.Add(gameDirectoryTextBox);
            Controls.Add(autoStartCheckBox);
            Name = "SettingsForm";
            Text = "Settings";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.CheckBox autoStartCheckBox;
        private System.Windows.Forms.TextBox gameDirectoryTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button saveButton;
    }
}
