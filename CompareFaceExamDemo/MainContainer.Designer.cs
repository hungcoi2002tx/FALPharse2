namespace CompareFaceExamDemo
{
    partial class MainContainer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            viewImageSourceToolStripMenuItem = new ToolStripMenuItem();
            viewImageCaptureToolStripMenuItem = new ToolStripMenuItem();
            viewResultToolStripMenuItem = new ToolStripMenuItem();
            settingToolStripMenuItem = new ToolStripMenuItem();
            settingToolStripMenuItem1 = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { viewImageSourceToolStripMenuItem, viewImageCaptureToolStripMenuItem, viewResultToolStripMenuItem, settingToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 28);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // viewImageSourceToolStripMenuItem
            // 
            viewImageSourceToolStripMenuItem.Name = "viewImageSourceToolStripMenuItem";
            viewImageSourceToolStripMenuItem.Size = new Size(150, 24);
            viewImageSourceToolStripMenuItem.Text = "View Image Source";
            viewImageSourceToolStripMenuItem.Click += ImageSourceButtonClick;
            // 
            // viewImageCaptureToolStripMenuItem
            // 
            viewImageCaptureToolStripMenuItem.Name = "viewImageCaptureToolStripMenuItem";
            viewImageCaptureToolStripMenuItem.Size = new Size(157, 24);
            viewImageCaptureToolStripMenuItem.Text = "View Image Capture";
            viewImageCaptureToolStripMenuItem.Click += ImageCaptureButtonClick;
            // 
            // viewResultToolStripMenuItem
            // 
            viewResultToolStripMenuItem.Name = "viewResultToolStripMenuItem";
            viewResultToolStripMenuItem.Size = new Size(99, 24);
            viewResultToolStripMenuItem.Text = "View Result";
            viewResultToolStripMenuItem.Click += ViewResultButtonClick;
            // 
            // settingToolStripMenuItem
            // 
            settingToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { settingToolStripMenuItem1, exitToolStripMenuItem });
            settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            settingToolStripMenuItem.Size = new Size(55, 24);
            settingToolStripMenuItem.Text = "Help";
            settingToolStripMenuItem.Click += settingToolStripMenuItem_Click;
            // 
            // settingToolStripMenuItem1
            // 
            settingToolStripMenuItem1.Name = "settingToolStripMenuItem1";
            settingToolStripMenuItem1.Size = new Size(224, 26);
            settingToolStripMenuItem1.Text = "Setting";
            settingToolStripMenuItem1.Click += SettingButtonClick;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(224, 26);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += ExitButtonClick;
            // 
            // MainContainer
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(menuStrip1);
            IsMdiContainer = true;
            MainMenuStrip = menuStrip1;
            Name = "MainContainer";
            Text = "MainContainer";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem viewImageSourceToolStripMenuItem;
        private ToolStripMenuItem viewImageCaptureToolStripMenuItem;
        private ToolStripMenuItem viewResultToolStripMenuItem;
        private ToolStripMenuItem settingToolStripMenuItem;
        private ToolStripMenuItem settingToolStripMenuItem1;
        private ToolStripMenuItem exitToolStripMenuItem;
    }
}