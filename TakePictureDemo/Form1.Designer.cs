namespace TakePictureDemo
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
            button1 = new Button();
            textBox1 = new TextBox();
            sendBtn = new Button();
            txtBoxToken = new TextBox();
            label1 = new Label();
            label2 = new Label();
            txtResult = new TextBox();
            label3 = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(380, 114);
            button1.Name = "button1";
            button1.Size = new Size(121, 23);
            button1.TabIndex = 0;
            button1.Text = "choose picture";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(154, 115);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(220, 23);
            textBox1.TabIndex = 1;
            // 
            // sendBtn
            // 
            sendBtn.Location = new Point(252, 258);
            sendBtn.Name = "sendBtn";
            sendBtn.Size = new Size(160, 23);
            sendBtn.TabIndex = 2;
            sendBtn.Text = "send";
            sendBtn.UseVisualStyleBackColor = true;
            sendBtn.Click += sendBtn_Click;
            // 
            // txtBoxToken
            // 
            txtBoxToken.Location = new Point(154, 61);
            txtBoxToken.Name = "txtBoxToken";
            txtBoxToken.Size = new Size(220, 23);
            txtBoxToken.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(154, 43);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 4;
            label1.Text = "Token";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(154, 97);
            label2.Name = "label2";
            label2.Size = new Size(109, 15);
            label2.TabIndex = 5;
            label2.Text = "picture source path";
            // 
            // txtResult
            // 
            txtResult.Location = new Point(154, 170);
            txtResult.Name = "txtResult";
            txtResult.Size = new Size(217, 23);
            txtResult.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(154, 152);
            label3.Name = "label3";
            label3.Size = new Size(36, 15);
            label3.TabIndex = 7;
            label3.Text = "result";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 338);
            Controls.Add(label3);
            Controls.Add(txtResult);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtBoxToken);
            Controls.Add(sendBtn);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox textBox1;
        private Button sendBtn;
        private TextBox txtBoxToken;
        private Label label1;
        private Label label2;
        private TextBox txtResult;
        private Label label3;
    }
}
