namespace AuthenExamCompareFaceExam
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
            imageSourceToolStripMenuItem = new ToolStripMenuItem();
            imageCaptureToolStripMenuItem = new ToolStripMenuItem();
            resultToolStripMenuItem = new ToolStripMenuItem();
            settingToolStripMenuItem = new ToolStripMenuItem();
            settingToolStripMenuItem1 = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { viewImageSourceToolStripMenuItem, settingToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(6, 3, 0, 3);
            menuStrip1.Size = new Size(800, 30);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // viewImageSourceToolStripMenuItem
            // 
            viewImageSourceToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { imageSourceToolStripMenuItem, imageCaptureToolStripMenuItem, resultToolStripMenuItem });
            viewImageSourceToolStripMenuItem.Name = "viewImageSourceToolStripMenuItem";
            viewImageSourceToolStripMenuItem.Size = new Size(77, 24);
            viewImageSourceToolStripMenuItem.Text = "Manage";
            // 
            // imageSourceToolStripMenuItem
            // 
            imageSourceToolStripMenuItem.Name = "imageSourceToolStripMenuItem";
            imageSourceToolStripMenuItem.Size = new Size(224, 26);
            imageSourceToolStripMenuItem.Text = "ImageSource";
            imageSourceToolStripMenuItem.Click += ImageSourceButtonClick;
            // 
            // imageCaptureToolStripMenuItem
            // 
            imageCaptureToolStripMenuItem.Name = "imageCaptureToolStripMenuItem";
            imageCaptureToolStripMenuItem.Size = new Size(224, 26);
            imageCaptureToolStripMenuItem.Text = "Compage Image";
            imageCaptureToolStripMenuItem.Click += ImageCaptureButtonClick;
            // 
            // resultToolStripMenuItem
            // 
            resultToolStripMenuItem.Name = "resultToolStripMenuItem";
            resultToolStripMenuItem.Size = new Size(224, 26);
            resultToolStripMenuItem.Text = "Result";
            resultToolStripMenuItem.Click += ViewResultButtonClick;
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
            settingToolStripMenuItem1.Size = new Size(139, 26);
            settingToolStripMenuItem1.Text = "Setting";
            settingToolStripMenuItem1.Click += SettingButtonClick;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(139, 26);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += ExitButtonClick;
            // 
            // MainContainer
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 464);
            Controls.Add(menuStrip1);
            IsMdiContainer = true;
            MainMenuStrip = menuStrip1;
            Name = "MainContainer";
            Text = "EOS Face Verification";
            Load += MainContainer_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem viewImageSourceToolStripMenuItem;
        private ToolStripMenuItem settingToolStripMenuItem;
        private ToolStripMenuItem settingToolStripMenuItem1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem imageSourceToolStripMenuItem;
        private ToolStripMenuItem imageCaptureToolStripMenuItem;
        private ToolStripMenuItem resultToolStripMenuItem;
    }
}