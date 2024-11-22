namespace CompareFaceExamDemo
{
    partial class Test : Form
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
            this.textBoxDirectory = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // 
            // textBoxDirectory
            // 
            this.textBoxDirectory.Location = new System.Drawing.Point(50, 50);
            this.textBoxDirectory.Size = new System.Drawing.Size(600, 25);
            this.textBoxDirectory.ReadOnly = true; // Không cho chỉnh sửa trực tiếp

            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(670, 50);
            this.buttonBrowse.Size = new System.Drawing.Size(100, 25);
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);

            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Location = new System.Drawing.Point(50, 100);
            this.buttonGenerate.Size = new System.Drawing.Size(200, 40);
            this.buttonGenerate.Text = "Generate Code";
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);

            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(300, 100);
            this.buttonClose.Size = new System.Drawing.Size(200, 40);
            this.buttonClose.Text = "Close";
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);

            // 
            // SettingForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 200);
            this.Controls.Add(this.textBoxDirectory);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.buttonClose);
            this.Text = "Folder Selection Example";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox textBoxDirectory;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Button buttonClose;
    }
}
