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
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 218);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1021, 447);
            dataGridView1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(939, 165);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 1;
            button1.Text = "Load";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // txtStudentCode
            // 
            txtStudentCode.Location = new Point(116, 125);
            txtStudentCode.Name = "txtStudentCode";
            txtStudentCode.Size = new Size(250, 27);
            txtStudentCode.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(11, 128);
            label1.Name = "label1";
            label1.Size = new Size(99, 20);
            label1.TabIndex = 3;
            label1.Text = "Student Code";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(61, 95);
            label2.Name = "label2";
            label2.Size = new Size(49, 20);
            label2.TabIndex = 4;
            label2.Text = "Status";
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(116, 91);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(250, 28);
            cmbStatus.TabIndex = 5;
            // 
            // cmbSortField
            // 
            cmbSortField.FormattingEnabled = true;
            cmbSortField.Location = new Point(510, 92);
            cmbSortField.Name = "cmbSortField";
            cmbSortField.Size = new Size(125, 28);
            cmbSortField.TabIndex = 5;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(451, 96);
            label5.Name = "label5";
            label5.Size = new Size(56, 20);
            label5.TabIndex = 4;
            label5.Text = "Sort By";
            // 
            // chkSortDesc
            // 
            chkSortDesc.AutoSize = true;
            chkSortDesc.Location = new Point(641, 95);
            chkSortDesc.Name = "chkSortDesc";
            chkSortDesc.Size = new Size(63, 24);
            chkSortDesc.TabIndex = 9;
            chkSortDesc.Text = "Desc";
            chkSortDesc.UseVisualStyleBackColor = true;
            // 
            // txtExamCode
            // 
            txtExamCode.Location = new Point(116, 162);
            txtExamCode.Name = "txtExamCode";
            txtExamCode.Size = new Size(250, 27);
            txtExamCode.TabIndex = 2;
            txtExamCode.Visible = false;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(26, 165);
            label6.Name = "label6";
            label6.Size = new Size(84, 20);
            label6.TabIndex = 3;
            label6.Text = "Exam Code";
            label6.Visible = false;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(394, 130);
            label4.Name = "label4";
            label4.Size = new Size(113, 20);
            label4.TabIndex = 8;
            label4.Text = "Min Confidence";
            // 
            // txtMinConfidence
            // 
            txtMinConfidence.Location = new Point(510, 128);
            txtMinConfidence.Name = "txtMinConfidence";
            txtMinConfidence.Size = new Size(125, 27);
            txtMinConfidence.TabIndex = 10;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(391, 163);
            label8.Name = "label8";
            label8.Size = new Size(116, 20);
            label8.TabIndex = 8;
            label8.Text = "Max Confidence";
            // 
            // txtMaxConfidence
            // 
            txtMaxConfidence.Location = new Point(510, 161);
            txtMaxConfidence.Name = "txtMaxConfidence";
            txtMaxConfidence.Size = new Size(125, 27);
            txtMaxConfidence.TabIndex = 10;
            txtMaxConfidence.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // pictureBoxTargetImage
            // 
            pictureBoxTargetImage.Location = new Point(1050, 218);
            pictureBoxTargetImage.Name = "pictureBoxTargetImage";
            pictureBoxTargetImage.Size = new Size(358, 218);
            pictureBoxTargetImage.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxTargetImage.TabIndex = 11;
            pictureBoxTargetImage.TabStop = false;
            // 
            // pictureBoxSourceImage
            // 
            pictureBoxSourceImage.Location = new Point(1050, 442);
            pictureBoxSourceImage.Name = "pictureBoxSourceImage";
            pictureBoxSourceImage.Size = new Size(358, 223);
            pictureBoxSourceImage.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSourceImage.TabIndex = 11;
            pictureBoxSourceImage.TabStop = false;
            // 
            // txtDataFolder
            // 
            txtDataFolder.Location = new Point(116, 27);
            txtDataFolder.Name = "txtDataFolder";
            txtDataFolder.ReadOnly = true;
            txtDataFolder.Size = new Size(250, 27);
            txtDataFolder.TabIndex = 13;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(26, 30);
            label9.Name = "label9";
            label9.Size = new Size(85, 20);
            label9.TabIndex = 7;
            label9.Text = "Data folder";
            // 
            // button3
            // 
            button3.Location = new Point(372, 26);
            button3.Name = "button3";
            button3.Size = new Size(94, 29);
            button3.TabIndex = 14;
            button3.Text = "Choose";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // cmbFileList
            // 
            cmbFileList.FormattingEnabled = true;
            cmbFileList.Location = new Point(583, 26);
            cmbFileList.Name = "cmbFileList";
            cmbFileList.Size = new Size(278, 28);
            cmbFileList.TabIndex = 15;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(497, 30);
            label3.Name = "label3";
            label3.Size = new Size(80, 20);
            label3.TabIndex = 16;
            label3.Text = "ExamCode";
            // 
            // btnExport
            // 
            btnExport.Location = new Point(793, 165);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(140, 29);
            btnExport.TabIndex = 17;
            btnExport.Text = "Export to Excel";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1420, 690);
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
            Name = "Main";
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
