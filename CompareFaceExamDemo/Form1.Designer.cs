namespace CompareFaceExamDemo
{
    partial class Form1
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
            dtpStartDate = new DateTimePicker();
            dtpEndDate = new DateTimePicker();
            label3 = new Label();
            label4 = new Label();
            cmbSortField = new ComboBox();
            label5 = new Label();
            chkSortDesc = new CheckBox();
            txtExamCode = new TextBox();
            label6 = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 218);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(1060, 405);
            dataGridView1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(924, 57);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 1;
            button1.Text = "Load";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // txtStudentCode
            // 
            txtStudentCode.Location = new Point(116, 63);
            txtStudentCode.Name = "txtStudentCode";
            txtStudentCode.Size = new Size(250, 27);
            txtStudentCode.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(11, 66);
            label1.Name = "label1";
            label1.Size = new Size(99, 20);
            label1.TabIndex = 3;
            label1.Text = "Student Code";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(61, 33);
            label2.Name = "label2";
            label2.Size = new Size(49, 20);
            label2.TabIndex = 4;
            label2.Text = "Status";
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(116, 29);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(250, 28);
            cmbStatus.TabIndex = 5;
            // 
            // dtpStartDate
            // 
            dtpStartDate.Location = new Point(466, 64);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(250, 27);
            dtpStartDate.TabIndex = 6;
            // 
            // dtpEndDate
            // 
            dtpEndDate.Location = new Point(466, 100);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(250, 27);
            dtpEndDate.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(380, 69);
            label3.Name = "label3";
            label3.Size = new Size(76, 20);
            label3.TabIndex = 7;
            label3.Text = "Start Date";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(386, 103);
            label4.Name = "label4";
            label4.Size = new Size(70, 20);
            label4.TabIndex = 8;
            label4.Text = "End Date";
            // 
            // cmbSortField
            // 
            cmbSortField.FormattingEnabled = true;
            cmbSortField.Location = new Point(466, 29);
            cmbSortField.Name = "cmbSortField";
            cmbSortField.Size = new Size(125, 28);
            cmbSortField.TabIndex = 5;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(404, 35);
            label5.Name = "label5";
            label5.Size = new Size(56, 20);
            label5.TabIndex = 4;
            label5.Text = "Sort By";
            // 
            // chkSortDesc
            // 
            chkSortDesc.AutoSize = true;
            chkSortDesc.Location = new Point(606, 32);
            chkSortDesc.Name = "chkSortDesc";
            chkSortDesc.Size = new Size(94, 24);
            chkSortDesc.TabIndex = 9;
            chkSortDesc.Text = "Sort Desc";
            chkSortDesc.UseVisualStyleBackColor = true;
            // 
            // txtExamCode
            // 
            txtExamCode.Location = new Point(116, 100);
            txtExamCode.Name = "txtExamCode";
            txtExamCode.Size = new Size(250, 27);
            txtExamCode.TabIndex = 2;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(11, 103);
            label6.Name = "label6";
            label6.Size = new Size(84, 20);
            label6.TabIndex = 3;
            label6.Text = "Exam Code";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 690);
            Controls.Add(chkSortDesc);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(dtpEndDate);
            Controls.Add(dtpStartDate);
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
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
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
        private Label label3;
        private Label label4;
        private ComboBox cmbSortField;
        private Label label5;
        private CheckBox chkSortDesc;
        private TextBox txtExamCode;
        private Label label6;
    }
}
