namespace TakePictureDemo
{
    partial class CameraForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CameraForm));
            pictureBox = new PictureBox();
            comboBoxCamera = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            textBox1 = new TextBox();
            label3 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // pictureBox
            // 
            pictureBox.Anchor = AnchorStyles.None;
            pictureBox.Location = new Point(541, 345);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(1280, 720);
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            // 
            // comboBoxCamera
            // 
            comboBoxCamera.FormattingEnabled = true;
            comboBoxCamera.Location = new Point(453, 54);
            comboBoxCamera.Name = "comboBoxCamera";
            comboBoxCamera.Size = new Size(330, 23);
            comboBoxCamera.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(403, 58);
            label1.Name = "label1";
            label1.Size = new Size(48, 15);
            label1.TabIndex = 2;
            label1.Text = "Camera";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(385, 111);
            label2.Name = "label2";
            label2.Size = new Size(67, 15);
            label2.TabIndex = 3;
            label2.Text = "Image Path";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(453, 106);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(330, 23);
            textBox1.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(956, 54);
            label3.Name = "label3";
            label3.Size = new Size(274, 75);
            label3.TabIndex = 5;
            label3.Text = resources.GetString("label3.Text");
            // 
            // CameraForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1859, 895);
            Controls.Add(label3);
            Controls.Add(textBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(comboBoxCamera);
            Controls.Add(pictureBox);
            Name = "CameraForm";
            Text = "CameraForm";
            WindowState = FormWindowState.Maximized;
            Load += CameraForm_Load;
            Resize += CameraForm_Resize;
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox;
        private ComboBox comboBoxCamera;
        private Label label1;
        private Label label2;
        private TextBox textBox1;
        private Label label3;
    }
}