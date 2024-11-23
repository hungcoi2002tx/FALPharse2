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
            txtStudentCode = new TextBox();
            pictureBoxSourceImage = new PictureBox();
            btnSave = new Button();
            btnDelete = new Button();
            btnLoad = new Button();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSourceImage).BeginInit();
            SuspendLayout();
            // 
            // txtStudentCode
            // 
            txtStudentCode.Location = new Point(127, 12);
            txtStudentCode.Name = "txtStudentCode";
            txtStudentCode.Size = new Size(248, 27);
            txtStudentCode.TabIndex = 0;
            // 
            // pictureBoxSourceImage
            // 
            pictureBoxSourceImage.Location = new Point(12, 95);
            pictureBoxSourceImage.Name = "pictureBoxSourceImage";
            pictureBoxSourceImage.Size = new Size(468, 529);
            pictureBoxSourceImage.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSourceImage.TabIndex = 1;
            pictureBoxSourceImage.TabStop = false;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(176, 51);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(94, 38);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSaveImage_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(281, 51);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(94, 38);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDeleteImage_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(386, 9);
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
            label1.Location = new Point(22, 15);
            label1.Name = "label1";
            label1.Size = new Size(99, 20);
            label1.TabIndex = 3;
            label1.Text = "Student Code";
            // 
            // SourceImageForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(492, 636);
            Controls.Add(label1);
            Controls.Add(btnDelete);
            Controls.Add(btnLoad);
            Controls.Add(btnSave);
            Controls.Add(pictureBoxSourceImage);
            Controls.Add(txtStudentCode);
            Name = "SourceImageForm";
            Text = "Form1";
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
    }
}