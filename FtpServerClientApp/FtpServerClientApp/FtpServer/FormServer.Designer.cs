namespace FtpServer
{
    partial class FormServer
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
            this.txt_Status = new System.Windows.Forms.TextBox();
            this.btn_Start = new System.Windows.Forms.Button();
            this.btn_Close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txt_Status
            // 
            this.txt_Status.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_Status.Location = new System.Drawing.Point(43, 54);
            this.txt_Status.Name = "txt_Status";
            this.txt_Status.ReadOnly = true;
            this.txt_Status.Size = new System.Drawing.Size(291, 30);
            this.txt_Status.TabIndex = 0;
            // 
            // btn_Start
            // 
            this.btn_Start.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Start.Location = new System.Drawing.Point(43, 124);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(127, 45);
            this.btn_Start.TabIndex = 1;
            this.btn_Start.Text = "Start server";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // btn_Close
            // 
            this.btn_Close.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Close.Location = new System.Drawing.Point(198, 124);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(136, 45);
            this.btn_Close.TabIndex = 2;
            this.btn_Close.Text = "Close server";
            this.btn_Close.UseVisualStyleBackColor = true;
            // 
            // FormServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 244);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.btn_Start);
            this.Controls.Add(this.txt_Status);
            this.Name = "FormServer";
            this.Text = "Ftp Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_Status;
        private System.Windows.Forms.Button btn_Start;
        private System.Windows.Forms.Button btn_Close;
    }
}

