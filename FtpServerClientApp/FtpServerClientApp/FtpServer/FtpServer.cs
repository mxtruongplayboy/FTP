using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FtpServer
{
    public class FtpServer
    {
        private string _host;
        private string _port;
        Socket socket;
        public FtpServer(string host, string port)
        {
            _host = host;
            _port = port;
        }

        public void ReceiveFile()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(_host), int.Parse(_port)));
            socket.Listen(10);

            try
            {
                while (true)
                {
                    Socket clientSocket = socket.Accept();
                    string filePath = @"E:\";
                    byte[] buffer = new byte[1024];

                    // Receive file extension
                    string fileExtension = Encoding.ASCII.GetString(buffer, 0, clientSocket.Receive(buffer));
                    filePath += @"\" + Guid.NewGuid() + fileExtension;

                    // Receive file data
                    int bytesRead;
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        while ((bytesRead = clientSocket.Receive(buffer)) > 0)
                        {
                            fs.Write(buffer, 0, bytesRead);
                        }
                    }

                    clientSocket.Close();
                }
            }
            catch (SocketException ex)
            {
                socket.Close();
                MessageBox.Show(ex.Message);
            }
        }
           

        public void CloseServer()
        {
            if (socket != null)
            {
                socket.Close();
            }
        }
    }
}
