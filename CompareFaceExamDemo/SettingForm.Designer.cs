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
            ((System.ComponentModel.ISupportInitialize)numberConfident).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numberThread).BeginInit();
            SuspendLayout();
            // 
            // labelConfident
            // 
            labelConfident.Location = new Point(29, 56);
            labelConfident.Name = "labelConfident";
            labelConfident.Size = new Size(153, 40);
            labelConfident.TabIndex = 1;
            labelConfident.Text = "Confident:";
            // 
            // labelDirectory
            // 
            labelDirectory.Location = new Point(29, 123);
            labelDirectory.Name = "labelDirectory";
            labelDirectory.Size = new Size(153, 40);
            labelDirectory.TabIndex = 3;
            labelDirectory.Text = "Directory Image Source:";
            labelDirectory.Click += labelDirectory_Click;
            // 
            // txtDirectorySource
            // 
            txtDirectorySource.Location = new Point(200, 123);
            txtDirectorySource.Margin = new Padding(3, 4, 3, 4);
            txtDirectorySource.Name = "txtDirectorySource";
            txtDirectorySource.ReadOnly = true;
            txtDirectorySource.Size = new Size(694, 27);
            txtDirectorySource.TabIndex = 4;
            txtDirectorySource.TextChanged += textBoxDirectorySource_TextChanged;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(1011, 613);
            btnSave.Margin = new Padding(3, 4, 3, 4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(144, 40);
            btnSave.TabIndex = 10;
            btnSave.Text = "Save";
            btnSave.Click += Btn_Save_Click;
            // 
            // btnBrowseSource
            // 
            btnBrowseSource.Location = new Point(961, 121);
            btnBrowseSource.Margin = new Padding(3, 4, 3, 4);
            btnBrowseSource.Name = "btnBrowseSource";
            btnBrowseSource.Size = new Size(114, 31);
            btnBrowseSource.TabIndex = 13;
            btnBrowseSource.Text = "Browse";
            btnBrowseSource.UseVisualStyleBackColor = true;
            btnBrowseSource.Click += Btn_Browse_Source_Click;
            // 
            // numberConfident
            // 
            numberConfident.Location = new Point(200, 53);
            numberConfident.Margin = new Padding(3, 4, 3, 4);
            numberConfident.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numberConfident.Name = "numberConfident";
            numberConfident.Size = new Size(137, 27);
            numberConfident.TabIndex = 14;
            numberConfident.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(416, 56);
            label1.Name = "label1";
            label1.Size = new Size(87, 20);
            label1.TabIndex = 15;
            label1.Text = "Thread Run:";
            // 
            // numberThread
            // 
            numberThread.Location = new Point(535, 53);
            numberThread.Margin = new Padding(3, 4, 3, 4);
            numberThread.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numberThread.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numberThread.Name = "numberThread";
            numberThread.Size = new Size(137, 27);
            numberThread.TabIndex = 16;
            numberThread.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // SettingForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1182, 753);
            Controls.Add(numberThread);
            Controls.Add(label1);
            Controls.Add(numberConfident);
            Controls.Add(btnBrowseSource);
            Controls.Add(labelConfident);
            Controls.Add(labelDirectory);
            Controls.Add(txtDirectorySource);
            Controls.Add(btnSave);
            Margin = new Padding(3, 4, 3, 4);
            Name = "SettingForm";
            Text = "Setting Form";
            ((System.ComponentModel.ISupportInitialize)numberConfident).EndInit();
            ((System.ComponentModel.ISupportInitialize)numberThread).EndInit();
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
    }
}
