using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FtpServer
{
    public partial class FormServer : Form
    {
        private FtpServer _ftpServer;
        public FormServer()
        {
            InitializeComponent();

            string _host = "127.0.0.1";
            string _port = "2003";
            _ftpServer = new FtpServer(_host, _port);
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            _ftpServer.ReceiveFile();
        }
    }
}
