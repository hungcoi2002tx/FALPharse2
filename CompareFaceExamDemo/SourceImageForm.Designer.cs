namespace AuthenExamCompareFaceExam
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
            dataGridViewSourceImage = new DataGridView();
            btnLoadDataSource = new Button();
            btnToggleSelectionMode = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSourceImage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewSourceImage).BeginInit();
            SuspendLayout();
            // 
            // txtStudentCode
            // 
            txtStudentCode.Location = new Point(155, 24);
            txtStudentCode.Margin = new Padding(3, 2, 3, 2);
            txtStudentCode.Name = "txtStudentCode";
            txtStudentCode.Size = new Size(218, 23);
            txtStudentCode.TabIndex = 0;
            // 
            // pictureBoxSourceImage
            // 
            pictureBoxSourceImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBoxSourceImage.BackColor = SystemColors.ActiveBorder;
            pictureBoxSourceImage.Location = new Point(936, 68);
            pictureBoxSourceImage.Margin = new Padding(3, 2, 3, 2);
            pictureBoxSourceImage.Name = "pictureBoxSourceImage";
            pictureBoxSourceImage.Size = new Size(450, 508);
            pictureBoxSourceImage.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSourceImage.TabIndex = 1;
            pictureBoxSourceImage.TabStop = false;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.BackColor = Color.ForestGreen;
            btnSave.Location = new Point(1159, 593);
            btnSave.Margin = new Padding(3, 2, 3, 2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(106, 57);
            btnSave.TabIndex = 2;
            btnSave.Text = "Add";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSaveImage_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDelete.BackColor = Color.IndianRed;
            btnDelete.Location = new Point(1029, 593);
            btnDelete.Margin = new Padding(3, 2, 3, 2);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(106, 57);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDeleteImage_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(379, 24);
            btnLoad.Margin = new Padding(3, 2, 3, 2);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(82, 24);
            btnLoad.TabIndex = 2;
            btnLoad.Text = "Search";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoadImage_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 29);
            label1.Name = "label1";
            label1.Size = new Size(133, 15);
            label1.TabIndex = 3;
            label1.Text = "Search by Student Code";
            // 
            // btnImport
            // 
            btnImport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnImport.Location = new Point(1085, 11);
            btnImport.Margin = new Padding(3, 2, 3, 2);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(106, 50);
            btnImport.TabIndex = 4;
            btnImport.Text = "Import Folder";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // btnDeleteBulk
            // 
            btnDeleteBulk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDeleteBulk.Location = new Point(1212, 11);
            btnDeleteBulk.Margin = new Padding(3, 2, 3, 2);
            btnDeleteBulk.Name = "btnDeleteBulk";
            btnDeleteBulk.Size = new Size(106, 50);
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
            // dataGridViewSourceImage
            // 
            dataGridViewSourceImage.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridViewSourceImage.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewSourceImage.Location = new Point(1, 68);
            dataGridViewSourceImage.Name = "dataGridViewSourceImage";
            dataGridViewSourceImage.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            dataGridViewSourceImage.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridViewSourceImage.Size = new Size(929, 600);
            dataGridViewSourceImage.TabIndex = 5;
            dataGridViewSourceImage.SelectionChanged += dataGridViewSourceImage_SelectionChanged;
            // 
            // btnLoadDataSource
            // 
            btnLoadDataSource.BackColor = SystemColors.ActiveCaption;
            btnLoadDataSource.Location = new Point(773, 8);
            btnLoadDataSource.Margin = new Padding(3, 2, 3, 2);
            btnLoadDataSource.Name = "btnLoadDataSource";
            btnLoadDataSource.Size = new Size(157, 52);
            btnLoadDataSource.TabIndex = 6;
            btnLoadDataSource.Text = "Load Data Source";
            btnLoadDataSource.UseVisualStyleBackColor = false;
            btnLoadDataSource.Click += btnLoadDataSource_Click;
            // 
            // btnToggleSelectionMode
            // 
            btnToggleSelectionMode.Location = new Point(619, 8);
            btnToggleSelectionMode.Margin = new Padding(3, 2, 3, 2);
            btnToggleSelectionMode.Name = "btnToggleSelectionMode";
            btnToggleSelectionMode.Size = new Size(106, 50);
            btnToggleSelectionMode.TabIndex = 7;
            btnToggleSelectionMode.Text = "CellSelect";
            btnToggleSelectionMode.UseVisualStyleBackColor = true;
            btnToggleSelectionMode.Click += btnToggleSelectionMode_Click;
            // 
            // SourceImageForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1349, 671);
            Controls.Add(btnToggleSelectionMode);
            Controls.Add(btnLoadDataSource);
            Controls.Add(dataGridViewSourceImage);
            Controls.Add(btnDeleteBulk);
            Controls.Add(btnImport);
            Controls.Add(label1);
            Controls.Add(btnDelete);
            Controls.Add(btnLoad);
            Controls.Add(btnSave);
            Controls.Add(pictureBoxSourceImage);
            Controls.Add(txtStudentCode);
            Margin = new Padding(3, 2, 3, 2);
            Name = "SourceImageForm";
            Text = "Form1";
            Load += SourceImageForm_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxSourceImage).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewSourceImage).EndInit();
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
        private DataGridView dataGridViewSourceImage;
        private Button btnLoadDataSource;
        private Button btnToggleSelectionMode;
    }
}