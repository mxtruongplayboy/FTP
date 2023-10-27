namespace FtpClient
{
    partial class FormClient
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
            this.txt_FilePath = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_SendFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txt_FilePath
            // 
            this.txt_FilePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_FilePath.Location = new System.Drawing.Point(43, 23);
            this.txt_FilePath.Multiline = true;
            this.txt_FilePath.Name = "txt_FilePath";
            this.txt_FilePath.ReadOnly = true;
            this.txt_FilePath.Size = new System.Drawing.Size(371, 144);
            this.txt_FilePath.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(43, 189);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 40);
            this.button1.TabIndex = 1;
            this.button1.Text = "Open file";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_SendFile
            // 
            this.btn_SendFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_SendFile.Location = new System.Drawing.Point(203, 189);
            this.btn_SendFile.Name = "btn_SendFile";
            this.btn_SendFile.Size = new System.Drawing.Size(211, 40);
            this.btn_SendFile.TabIndex = 2;
            this.btn_SendFile.Text = "Save file to server";
            this.btn_SendFile.UseVisualStyleBackColor = true;
            this.btn_SendFile.Click += new System.EventHandler(this.btn_SendFile_Click);
            // 
            // FormClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 252);
            this.Controls.Add(this.btn_SendFile);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txt_FilePath);
            this.Name = "FormClient";
            this.Text = "Ftp Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_FilePath;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_SendFile;
    }
}

