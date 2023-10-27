using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FtpClient
{
    public partial class FormClient : Form
    {
        public FormClient()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txt_FilePath.Text = ofd.FileName;
            }
        }

        private void btn_SendFile_Click(object sender, EventArgs e)
        {
            string host = "127.0.0.1";
            string port = "2003";
            FtpClient ftpClient = new FtpClient(host, port);
            string filePath = txt_FilePath.Text;
            if (filePath != "")
            {
                ftpClient.SendFile(filePath);
            }
            else
            {
                MessageBox.Show("Please select a file to send");
            }
        }
    }
}
