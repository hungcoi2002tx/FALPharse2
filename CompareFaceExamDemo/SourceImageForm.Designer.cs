namespace CompareFaceExamDemo
{
    partial class SourceImageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SourceImageForm));
            txtStudentCode = new TextBox();
            pictureBoxSourceImage = new PictureBox();
            btnSave = new Button();
            btnDelete = new Button();
            btnLoad = new Button();
            label1 = new Label();
            btnImport = new Button();
            btnDeleteBulk = new Button();
            printPreviewDialog1 = new PrintPreviewDialog();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSourceImage).BeginInit();
            SuspendLayout();
            // 
            // txtStudentCode
            // 
            txtStudentCode.Location = new Point(153, 36);
            txtStudentCode.Name = "txtStudentCode";
            txtStudentCode.Size = new Size(248, 27);
            txtStudentCode.TabIndex = 0;
            // 
            // pictureBoxSourceImage
            // 
            pictureBoxSourceImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBoxSourceImage.BackColor = SystemColors.ActiveBorder;
            pictureBoxSourceImage.Location = new Point(48, 91);
            pictureBoxSourceImage.Name = "pictureBoxSourceImage";
            pictureBoxSourceImage.Size = new Size(468, 529);
            pictureBoxSourceImage.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSourceImage.TabIndex = 1;
            pictureBoxSourceImage.TabStop = false;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(652, 645);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(121, 29);
            btnSave.TabIndex = 2;
            btnSave.Text = "Add";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSaveImage_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDelete.Location = new Point(517, 645);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(121, 29);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDeleteImage_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(412, 33);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(94, 32);
            btnLoad.TabIndex = 2;
            btnLoad.Text = "Load";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoadImage_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(48, 39);
            label1.Name = "label1";
            label1.Size = new Size(99, 20);
            label1.TabIndex = 3;
            label1.Text = "Student Code";
            // 
            // btnImport
            // 
            btnImport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnImport.Location = new Point(517, 36);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(121, 29);
            btnImport.TabIndex = 4;
            btnImport.Text = "Import Folder";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // btnDeleteBulk
            // 
            btnDeleteBulk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDeleteBulk.Location = new Point(652, 36);
            btnDeleteBulk.Name = "btnDeleteBulk";
            btnDeleteBulk.Size = new Size(121, 29);
            btnDeleteBulk.TabIndex = 4;
            btnDeleteBulk.Text = "Delete Bulk";
            btnDeleteBulk.UseVisualStyleBackColor = true;
            btnDeleteBulk.Click += btnDeleteBulk_Click;
            // 
            // printPreviewDialog1
            // 
            printPreviewDialog1.AutoScrollMargin = new Size(0, 0);
            printPreviewDialog1.AutoScrollMinSize = new Size(0, 0);
            printPreviewDialog1.ClientSize = new Size(400, 300);
            printPreviewDialog1.Enabled = true;
            printPreviewDialog1.Icon = (Icon)resources.GetObject("printPreviewDialog1.Icon");
            printPreviewDialog1.Name = "printPreviewDialog1";
            printPreviewDialog1.Visible = false;
            // 
            // SourceImageForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(807, 686);
            Controls.Add(btnDeleteBulk);
            Controls.Add(btnImport);
            Controls.Add(label1);
            Controls.Add(btnDelete);
            Controls.Add(btnLoad);
            Controls.Add(btnSave);
            Controls.Add(pictureBoxSourceImage);
            Controls.Add(txtStudentCode);
            Name = "SourceImageForm";
            Text = "Form1";
            Load += SourceImageForm_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxSourceImage).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtStudentCode;
        private PictureBox pictureBoxSourceImage;
        private Button btnSave;
        private Button btnDelete;
        private Button btnLoad;
        private Label label1;
        private Button btnImport;
        private Button btnDeleteBulk;
        private PrintPreviewDialog printPreviewDialog1;
    }
}