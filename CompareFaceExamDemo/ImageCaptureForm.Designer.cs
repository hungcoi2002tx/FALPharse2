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
            btnSelectFolder = new Button();
            btnSend = new Button();
            txtFolderPath = new TextBox();
            dataGridViewImages = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dataGridViewImages).BeginInit();
            SuspendLayout();
            // 
            // btnSelectFolder
            // 
            btnSelectFolder.Location = new Point(14, 14);
            btnSelectFolder.Margin = new Padding(4, 3, 4, 3);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.Size = new Size(175, 35);
            btnSelectFolder.TabIndex = 0;
            btnSelectFolder.Text = "Select Folder";
            btnSelectFolder.UseVisualStyleBackColor = true;
            btnSelectFolder.Click += btnSelectFolder_Click;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(210, 14);
            btnSend.Margin = new Padding(4, 3, 4, 3);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(175, 35);
            btnSend.TabIndex = 1;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtFolderPath
            // 
            txtFolderPath.Location = new Point(14, 58);
            txtFolderPath.Margin = new Padding(4, 3, 4, 3);
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.ReadOnly = true;
            txtFolderPath.Size = new Size(583, 23);
            txtFolderPath.TabIndex = 2;
            // 
            // dataGridViewImages
            // 
            dataGridViewImages.AllowUserToAddRows = false;
            dataGridViewImages.AllowUserToDeleteRows = false;
            dataGridViewImages.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewImages.Location = new Point(3, 92);
            dataGridViewImages.Margin = new Padding(4, 3, 4, 3);
            dataGridViewImages.Name = "dataGridViewImages";
            dataGridViewImages.Size = new Size(1173, 623);
            dataGridViewImages.TabIndex = 3;
            // 
            // ImageCaptureForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1341, 720);
            Controls.Add(txtFolderPath);
            Controls.Add(dataGridViewImages);
            Controls.Add(btnSend);
            Controls.Add(btnSelectFolder);
            Margin = new Padding(4, 3, 4, 3);
            Name = "ImageCaptureForm";
            Text = "Image Viewer with Preview";
            Load += ImageCaptureForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewImages).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}