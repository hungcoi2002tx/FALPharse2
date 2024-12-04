using System;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace AuthenExamTakePicture
{
    partial class InfoForm
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
            button1 = new Button();
            label1 = new Label();
            textBox2 = new TextBox();
            label4 = new Label();
            label5 = new Label();
            textBox3 = new TextBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(188, 230);
            button1.Name = "button1";
            button1.Size = new Size(121, 23);
            button1.TabIndex = 0;
            button1.Text = "Take a picture";
            button1.UseVisualStyleBackColor = true;
            button1.Click += captureBtn_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(86, 136);
            label1.Name = "label1";
            label1.Size = new Size(95, 15);
            label1.TabIndex = 8;
            label1.Text = "Student Number";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(86, 154);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(347, 23);
            textBox2.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 27.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.Location = new Point(156, 9);
            label4.Name = "label4";
            label4.Size = new Size(203, 50);
            label4.TabIndex = 11;
            label4.Text = "Infomation";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(86, 92);
            label5.Name = "label5";
            label5.Size = new Size(65, 15);
            label5.TabIndex = 12;
            label5.Text = "Exam code";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(86, 110);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(347, 23);
            textBox3.TabIndex = 13;
            // 
            // InfoForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(527, 321);
            Controls.Add(textBox3);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(textBox2);
            Controls.Add(label1);
            Controls.Add(button1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "InfoForm";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label label1;
        private TextBox textBox2;
        private Label label4;
        private Label label5;
        private TextBox textBox3;
    }
}
