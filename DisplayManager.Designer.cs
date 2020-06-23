namespace LetterReader
{
    partial class DisplayManager
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
            this.buttonLoadImage = new System.Windows.Forms.Button();
            this.textBoxImageFile = new System.Windows.Forms.TextBox();
            this.pictureBoxLine = new System.Windows.Forms.PictureBox();
            this.buttonNextImage = new System.Windows.Forms.Button();
            this.buttonNextFile = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBoxLetter = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLine)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonLoadImage
            // 
            this.buttonLoadImage.Location = new System.Drawing.Point(18, 18);
            this.buttonLoadImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonLoadImage.Name = "buttonLoadImage";
            this.buttonLoadImage.Size = new System.Drawing.Size(112, 35);
            this.buttonLoadImage.TabIndex = 0;
            this.buttonLoadImage.Text = "Load image";
            this.buttonLoadImage.UseVisualStyleBackColor = true;
            this.buttonLoadImage.Click += new System.EventHandler(this.ButtonLoadImage_Click);
            // 
            // textBoxImageFile
            // 
            this.textBoxImageFile.Location = new System.Drawing.Point(140, 18);
            this.textBoxImageFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxImageFile.Name = "textBoxImageFile";
            this.textBoxImageFile.Size = new System.Drawing.Size(566, 26);
            this.textBoxImageFile.TabIndex = 1;
            // 
            // pictureBoxLine
            // 
            this.pictureBoxLine.Location = new System.Drawing.Point(18, 63);
            this.pictureBoxLine.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBoxLine.Name = "pictureBoxLine";
            this.pictureBoxLine.Size = new System.Drawing.Size(813, 404);
            this.pictureBoxLine.TabIndex = 2;
            this.pictureBoxLine.TabStop = false;
            // 
            // buttonNextImage
            // 
            this.buttonNextImage.Location = new System.Drawing.Point(18, 477);
            this.buttonNextImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonNextImage.Name = "buttonNextImage";
            this.buttonNextImage.Size = new System.Drawing.Size(112, 35);
            this.buttonNextImage.TabIndex = 3;
            this.buttonNextImage.Text = "Next line";
            this.buttonNextImage.UseVisualStyleBackColor = true;
            this.buttonNextImage.Click += new System.EventHandler(this.ButtonNextImage_Click);
            // 
            // buttonNextFile
            // 
            this.buttonNextFile.Location = new System.Drawing.Point(718, 14);
            this.buttonNextFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonNextFile.Name = "buttonNextFile";
            this.buttonNextFile.Size = new System.Drawing.Size(112, 35);
            this.buttonNextFile.TabIndex = 4;
            this.buttonNextFile.Text = ">";
            this.buttonNextFile.UseVisualStyleBackColor = true;
            this.buttonNextFile.Click += new System.EventHandler(this.ButtonNextFile_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(18, 520);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 31);
            this.button1.TabIndex = 5;
            this.button1.Text = "Run tests";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(177, 477);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(199, 35);
            this.button2.TabIndex = 6;
            this.button2.Text = "Accept letter";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // textBoxLetter
            // 
            this.textBoxLetter.Location = new System.Drawing.Point(382, 481);
            this.textBoxLetter.Name = "textBoxLetter";
            this.textBoxLetter.Size = new System.Drawing.Size(177, 26);
            this.textBoxLetter.TabIndex = 7;
            // 
            // DisplayManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(849, 692);
            this.Controls.Add(this.textBoxLetter);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonNextFile);
            this.Controls.Add(this.buttonNextImage);
            this.Controls.Add(this.pictureBoxLine);
            this.Controls.Add(this.textBoxImageFile);
            this.Controls.Add(this.buttonLoadImage);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DisplayManager";
            this.Text = "DisplayManager";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLine)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonLoadImage;
        private System.Windows.Forms.TextBox textBoxImageFile;
        private System.Windows.Forms.PictureBox pictureBoxLine;
        private System.Windows.Forms.Button buttonNextImage;
        private System.Windows.Forms.Button buttonNextFile;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBoxLetter;
    }
}