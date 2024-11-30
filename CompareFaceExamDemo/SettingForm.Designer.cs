using System;

namespace CompareFaceExamDemo
{
    partial class SettingForm
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
            labelConfident = new Label();
            labelDirectory = new Label();
            txtDirectorySource = new TextBox();
            btnSave = new Button();
            btnBrowseSource = new Button();
            numberConfident = new NumericUpDown();
            label1 = new Label();
            numberThread = new NumericUpDown();
            label2 = new Label();
            txtRetry = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)numberConfident).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numberThread).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtRetry).BeginInit();
            SuspendLayout();
            // 
            // labelConfident
            // 
            labelConfident.Location = new Point(79, 42);
            labelConfident.Name = "labelConfident";
            labelConfident.Size = new Size(69, 30);
            labelConfident.TabIndex = 1;
            labelConfident.Text = "Confident:";
            // 
            // labelDirectory
            // 
            labelDirectory.Location = new Point(14, 221);
            labelDirectory.Name = "labelDirectory";
            labelDirectory.Size = new Size(134, 30);
            labelDirectory.TabIndex = 3;
            labelDirectory.Text = "Directory Image Source:";
            labelDirectory.Click += labelDirectory_Click;
            // 
            // txtDirectorySource
            // 
            txtDirectorySource.Location = new Point(175, 217);
            txtDirectorySource.Name = "txtDirectorySource";
            txtDirectorySource.ReadOnly = true;
            txtDirectorySource.Size = new Size(597, 23);
            txtDirectorySource.TabIndex = 4;
            txtDirectorySource.TextChanged += textBoxDirectorySource_TextChanged;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(394, 320);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(234, 30);
            btnSave.TabIndex = 10;
            btnSave.Text = "Save";
            btnSave.Click += Btn_Save_Click;
            // 
            // btnBrowseSource
            // 
            btnBrowseSource.Location = new Point(778, 217);
            btnBrowseSource.Name = "btnBrowseSource";
            btnBrowseSource.Size = new Size(126, 23);
            btnBrowseSource.TabIndex = 13;
            btnBrowseSource.Text = "Browse";
            btnBrowseSource.UseVisualStyleBackColor = true;
            btnBrowseSource.Click += Btn_Browse_Source_Click;
            // 
            // numberConfident
            // 
            numberConfident.Location = new Point(175, 40);
            numberConfident.Name = "numberConfident";
            numberConfident.Size = new Size(164, 23);
            numberConfident.TabIndex = 14;
            numberConfident.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(71, 86);
            label1.Name = "label1";
            label1.Size = new Size(71, 15);
            label1.TabIndex = 15;
            label1.Text = "Thread Run:";
            // 
            // numberThread
            // 
            numberThread.Location = new Point(175, 84);
            numberThread.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            numberThread.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numberThread.Name = "numberThread";
            numberThread.Size = new Size(164, 23);
            numberThread.TabIndex = 16;
            numberThread.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(105, 128);
            label2.Name = "label2";
            label2.Size = new Size(37, 15);
            label2.TabIndex = 17;
            label2.Text = "Retry:";
            label2.Click += label2_Click;
            // 
            // txtRetry
            // 
            txtRetry.Location = new Point(175, 126);
            txtRetry.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            txtRetry.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            txtRetry.Name = "txtRetry";
            txtRetry.Size = new Size(164, 23);
            txtRetry.TabIndex = 18;
            txtRetry.Value = new decimal(new int[] { 1, 0, 0, 0 });
            txtRetry.Click += txtRetry_Click;
            // 
            // SettingForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1034, 565);
            Controls.Add(txtRetry);
            Controls.Add(label2);
            Controls.Add(numberThread);
            Controls.Add(label1);
            Controls.Add(numberConfident);
            Controls.Add(btnBrowseSource);
            Controls.Add(labelConfident);
            Controls.Add(labelDirectory);
            Controls.Add(txtDirectorySource);
            Controls.Add(btnSave);
            Name = "SettingForm";
            Text = "Setting Form";
            ((System.ComponentModel.ISupportInitialize)numberConfident).EndInit();
            ((System.ComponentModel.ISupportInitialize)numberThread).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtRetry).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Label labelConfident;
        private System.Windows.Forms.Label labelDirectory;
        private System.Windows.Forms.TextBox txtDirectorySource;
        private System.Windows.Forms.Button btnSave;
        private Button btnBrowseSource;
        private NumericUpDown numberConfident;
        private Label label1;
        private NumericUpDown numberThread;
        private Label label2;
        private NumericUpDown txtRetry;
    }
}
