namespace CompareFaceExamDemo
{
    partial class ImageCaptureForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.DataGridView dataGridViewImages;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtFolderPath;
        private System.Windows.Forms.PictureBox pictureBoxPreview;


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
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtFolderPath = new System.Windows.Forms.TextBox();
            this.dataGridViewImages = new System.Windows.Forms.DataGridView();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewImages)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(12, 12);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(150, 30);
            this.btnSelectFolder.TabIndex = 0;
            this.btnSelectFolder.Text = "Select Folder";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(180, 12);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(150, 30);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtFolderPath
            // 
            this.txtFolderPath.Location = new System.Drawing.Point(12, 50);
            this.txtFolderPath.Name = "txtFolderPath";
            this.txtFolderPath.ReadOnly = true;
            this.txtFolderPath.Size = new System.Drawing.Size(500, 20);
            this.txtFolderPath.TabIndex = 2;
            // 
            // dataGridViewImages
            // 
            this.dataGridViewImages.AllowUserToAddRows = false;
            this.dataGridViewImages.AllowUserToDeleteRows = false;
            this.dataGridViewImages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewImages.Location = new System.Drawing.Point(12, 80);
            this.dataGridViewImages.Name = "dataGridViewImages";
            this.dataGridViewImages.Size = new System.Drawing.Size(350, 270);
            this.dataGridViewImages.TabIndex = 3;
            this.dataGridViewImages.SelectionChanged += new System.EventHandler(this.dataGridViewImages_SelectionChanged);
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Location = new System.Drawing.Point(380, 80);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(150, 150);
            this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPreview.TabIndex = 4;
            this.pictureBoxPreview.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 361);
            this.Controls.Add(this.pictureBoxPreview);
            this.Controls.Add(this.txtFolderPath);
            this.Controls.Add(this.dataGridViewImages);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnSelectFolder);
            this.Name = "Form1";
            this.Text = "Image Viewer with Preview";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewImages)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}