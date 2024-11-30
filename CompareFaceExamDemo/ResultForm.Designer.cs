namespace CompareFaceExamDemo
{
    partial class ResultForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView();
            button1 = new Button();
            txtStudentCode = new TextBox();
            label1 = new Label();
            label2 = new Label();
            cmbStatus = new ComboBox();
            cmbSortField = new ComboBox();
            label5 = new Label();
            chkSortDesc = new CheckBox();
            txtExamCode = new TextBox();
            label6 = new Label();
            label4 = new Label();
            txtMinConfidence = new NumericUpDown();
            label8 = new Label();
            txtMaxConfidence = new NumericUpDown();
            pictureBoxTargetImage = new PictureBox();
            pictureBoxSourceImage = new PictureBox();
            txtDataFolder = new TextBox();
            label9 = new Label();
            button3 = new Button();
            cmbFileList = new ComboBox();
            label3 = new Label();
            btnExport = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtMinConfidence).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtMaxConfidence).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTargetImage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSourceImage).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(2, 170);
            dataGridView1.Margin = new Padding(3, 2, 3, 2);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1547, 803);
            dataGridView1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(822, 124);
            button1.Margin = new Padding(3, 2, 3, 2);
            button1.Name = "button1";
            button1.Size = new Size(82, 22);
            button1.TabIndex = 1;
            button1.Text = "Load";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // txtStudentCode
            // 
            txtStudentCode.Location = new Point(102, 94);
            txtStudentCode.Margin = new Padding(3, 2, 3, 2);
            txtStudentCode.Name = "txtStudentCode";
            txtStudentCode.Size = new Size(219, 23);
            txtStudentCode.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 96);
            label1.Name = "label1";
            label1.Size = new Size(79, 15);
            label1.TabIndex = 3;
            label1.Text = "Student Code";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(53, 71);
            label2.Name = "label2";
            label2.Size = new Size(39, 15);
            label2.TabIndex = 4;
            label2.Text = "Status";
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(102, 68);
            cmbStatus.Margin = new Padding(3, 2, 3, 2);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(219, 23);
            cmbStatus.TabIndex = 5;
            // 
            // cmbSortField
            // 
            cmbSortField.FormattingEnabled = true;
            cmbSortField.Location = new Point(446, 69);
            cmbSortField.Margin = new Padding(3, 2, 3, 2);
            cmbSortField.Name = "cmbSortField";
            cmbSortField.Size = new Size(110, 23);
            cmbSortField.TabIndex = 5;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(395, 72);
            label5.Name = "label5";
            label5.Size = new Size(44, 15);
            label5.TabIndex = 4;
            label5.Text = "Sort By";
            // 
            // chkSortDesc
            // 
            chkSortDesc.AutoSize = true;
            chkSortDesc.Location = new Point(561, 71);
            chkSortDesc.Margin = new Padding(3, 2, 3, 2);
            chkSortDesc.Name = "chkSortDesc";
            chkSortDesc.Size = new Size(51, 19);
            chkSortDesc.TabIndex = 9;
            chkSortDesc.Text = "Desc";
            chkSortDesc.UseVisualStyleBackColor = true;
            // 
            // txtExamCode
            // 
            txtExamCode.Location = new Point(102, 122);
            txtExamCode.Margin = new Padding(3, 2, 3, 2);
            txtExamCode.Name = "txtExamCode";
            txtExamCode.Size = new Size(219, 23);
            txtExamCode.TabIndex = 2;
            txtExamCode.Visible = false;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(23, 124);
            label6.Name = "label6";
            label6.Size = new Size(66, 15);
            label6.TabIndex = 3;
            label6.Text = "Exam Code";
            label6.Visible = false;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(345, 98);
            label4.Name = "label4";
            label4.Size = new Size(92, 15);
            label4.TabIndex = 8;
            label4.Text = "Min Confidence";
            // 
            // txtMinConfidence
            // 
            txtMinConfidence.Location = new Point(446, 96);
            txtMinConfidence.Margin = new Padding(3, 2, 3, 2);
            txtMinConfidence.Name = "txtMinConfidence";
            txtMinConfidence.Size = new Size(109, 23);
            txtMinConfidence.TabIndex = 10;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(342, 122);
            label8.Name = "label8";
            label8.Size = new Size(93, 15);
            label8.TabIndex = 8;
            label8.Text = "Max Confidence";
            // 
            // txtMaxConfidence
            // 
            txtMaxConfidence.Location = new Point(446, 121);
            txtMaxConfidence.Margin = new Padding(3, 2, 3, 2);
            txtMaxConfidence.Name = "txtMaxConfidence";
            txtMaxConfidence.Size = new Size(109, 23);
            txtMaxConfidence.TabIndex = 10;
            txtMaxConfidence.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // pictureBoxTargetImage
            // 
            pictureBoxTargetImage.Location = new Point(995, 2);
            pictureBoxTargetImage.Margin = new Padding(3, 2, 3, 2);
            pictureBoxTargetImage.Name = "pictureBoxTargetImage";
            pictureBoxTargetImage.Size = new Size(128, 164);
            pictureBoxTargetImage.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxTargetImage.TabIndex = 11;
            pictureBoxTargetImage.TabStop = false;
            // 
            // pictureBoxSourceImage
            // 
            pictureBoxSourceImage.Location = new Point(1129, 2);
            pictureBoxSourceImage.Margin = new Padding(3, 2, 3, 2);
            pictureBoxSourceImage.Name = "pictureBoxSourceImage";
            pictureBoxSourceImage.Size = new Size(128, 164);
            pictureBoxSourceImage.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSourceImage.TabIndex = 11;
            pictureBoxSourceImage.TabStop = false;
            // 
            // txtDataFolder
            // 
            txtDataFolder.Location = new Point(102, 20);
            txtDataFolder.Margin = new Padding(3, 2, 3, 2);
            txtDataFolder.Name = "txtDataFolder";
            txtDataFolder.ReadOnly = true;
            txtDataFolder.Size = new Size(219, 23);
            txtDataFolder.TabIndex = 13;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(23, 22);
            label9.Name = "label9";
            label9.Size = new Size(65, 15);
            label9.TabIndex = 7;
            label9.Text = "Data folder";
            // 
            // button3
            // 
            button3.Location = new Point(326, 20);
            button3.Margin = new Padding(3, 2, 3, 2);
            button3.Name = "button3";
            button3.Size = new Size(82, 23);
            button3.TabIndex = 14;
            button3.Text = "Choose";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // cmbFileList
            // 
            cmbFileList.FormattingEnabled = true;
            cmbFileList.Location = new Point(510, 20);
            cmbFileList.Margin = new Padding(3, 2, 3, 2);
            cmbFileList.Name = "cmbFileList";
            cmbFileList.Size = new Size(394, 23);
            cmbFileList.TabIndex = 15;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(435, 22);
            label3.Name = "label3";
            label3.Size = new Size(63, 15);
            label3.TabIndex = 16;
            label3.Text = "ExamCode";
            // 
            // btnExport
            // 
            btnExport.Location = new Point(694, 124);
            btnExport.Margin = new Padding(3, 2, 3, 2);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(122, 22);
            btnExport.TabIndex = 17;
            btnExport.Text = "Export to Excel";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // ResultForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1550, 975);
            Controls.Add(btnExport);
            Controls.Add(label3);
            Controls.Add(cmbFileList);
            Controls.Add(button3);
            Controls.Add(txtDataFolder);
            Controls.Add(pictureBoxSourceImage);
            Controls.Add(pictureBoxTargetImage);
            Controls.Add(txtMaxConfidence);
            Controls.Add(txtMinConfidence);
            Controls.Add(label8);
            Controls.Add(label4);
            Controls.Add(chkSortDesc);
            Controls.Add(label9);
            Controls.Add(cmbSortField);
            Controls.Add(cmbStatus);
            Controls.Add(label5);
            Controls.Add(label2);
            Controls.Add(label6);
            Controls.Add(label1);
            Controls.Add(txtExamCode);
            Controls.Add(txtStudentCode);
            Controls.Add(button1);
            Controls.Add(dataGridView1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "ResultForm";
            Text = "Form1";
            Load += Main_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtMinConfidence).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtMaxConfidence).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTargetImage).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSourceImage).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private Button button1;
        private TextBox txtStudentCode;
        private Label label1;
        private Label label2;
        private ComboBox cmbStatus;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private Label label4;
        private ComboBox cmbSortField;
        private Label label5;
        private CheckBox chkSortDesc;
        private TextBox txtExamCode;
        private Label label6;
        private Label label8;
        private NumericUpDown txtMinConfidence;
        private NumericUpDown txtMaxConfidence;
        private PictureBox pictureBoxTargetImage;
        private PictureBox pictureBoxSourceImage;
        private TextBox txtDataFolder;
        private Label label9;
        private Button button3;
        private ComboBox cmbFileList;
        private Label label3;
        private Button btnExport;
    }
}
