using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FtpClient
{
    public class FtpClient
    {
        private string _host;
        private string _port;
        public FtpClient(string host, string port)
        {
            _host = host;
            _port = port;
        }

        public void SendFile(string filePath)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(_host, int.Parse(_port));
            byte[] buffer = new byte[1024];
            int length;

            // Send file extension
            string fileExtension = Path.GetExtension(filePath);
            socket.Send(Encoding.ASCII.GetBytes(fileExtension));
            
            // Send file data
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                while ((length = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    socket.Send(buffer, length, SocketFlags.None);
                }
            }
        }
    }
}
