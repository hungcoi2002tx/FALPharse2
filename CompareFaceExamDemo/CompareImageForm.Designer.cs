namespace AuthenExamCompareFace
{
    partial class CompareImageForm
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
            progressBarCompare = new ProgressBar();
            lblProgress = new Label();
            button1 = new Button();
            button2 = new Button();
            btnStop = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewImages).BeginInit();
            SuspendLayout();
            // 
            // btnSelectFolder
            // 
            btnSelectFolder.Location = new Point(14, 14);
            btnSelectFolder.Margin = new Padding(4, 3, 4, 3);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.Size = new Size(137, 35);
            btnSelectFolder.TabIndex = 0;
            btnSelectFolder.Text = "Select Folder";
            btnSelectFolder.UseVisualStyleBackColor = true;
            btnSelectFolder.Click += btnSelectFolder_Click;
            // 
            // btnSend
            // 
            btnSend.Enabled = false;
            btnSend.Location = new Point(181, 14);
            btnSend.Margin = new Padding(4, 3, 4, 3);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(137, 35);
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
            txtFolderPath.Size = new Size(790, 23);
            txtFolderPath.TabIndex = 2;
            // 
            // dataGridViewImages
            // 
            dataGridViewImages.AllowUserToAddRows = false;
            dataGridViewImages.AllowUserToDeleteRows = false;
            dataGridViewImages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewImages.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridViewImages.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewImages.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewImages.Location = new Point(1, 104);
            dataGridViewImages.Margin = new Padding(4, 3, 4, 3);
            dataGridViewImages.Name = "dataGridViewImages";
            dataGridViewImages.Size = new Size(1783, 615);
            dataGridViewImages.TabIndex = 3;
            // 
            // progressBarCompare
            // 
            progressBarCompare.Location = new Point(923, 58);
            progressBarCompare.Name = "progressBarCompare";
            progressBarCompare.Size = new Size(460, 23);
            progressBarCompare.TabIndex = 4;
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(1136, 37);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(23, 15);
            lblProgress.TabIndex = 5;
            lblProgress.Text = "0%";
            // 
            // button1
            // 
            button1.Enabled = false;
            button1.Location = new Point(345, 14);
            button1.Name = "button1";
            button1.Size = new Size(137, 35);
            button1.TabIndex = 6;
            button1.Text = "Pause";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Enabled = false;
            button2.Location = new Point(506, 14);
            button2.Name = "button2";
            button2.Size = new Size(137, 35);
            button2.TabIndex = 7;
            button2.Text = "Resume";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Location = new Point(667, 14);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(137, 35);
            btnStop.TabIndex = 8;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // CompareImageForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1787, 720);
            Controls.Add(btnStop);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(lblProgress);
            Controls.Add(progressBarCompare);
            Controls.Add(txtFolderPath);
            Controls.Add(dataGridViewImages);
            Controls.Add(btnSend);
            Controls.Add(btnSelectFolder);
            Margin = new Padding(4, 3, 4, 3);
            Name = "CompareImageForm";
            Text = "Image Viewer with Preview";
            Load += ImageCaptureForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewImages).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private ProgressBar progressBarCompare;
        private Label lblProgress;
        private Button button1;
        private Button button2;
        private Button btnStop;
    }
}