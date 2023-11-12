using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPClient
{
    public class FTPSession
    {
        #region Properties
        TcpClient _controlSocket = new TcpClient();
        StreamWriter _writer;
        StreamReader _reader;
        public bool status = true;
        public IPAddress Address { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool Connected => _controlSocket.Connected;
        public bool LoggedOn { get; private set; }
        #endregion
        #region command
        public FTPSession(IPAddress Address, string User,string Password)
        {
            this.Address = Address; 
            this.User = User;
            this.Password = Password;
        }
        public void Connect()
        {
            if (!_controlSocket.Connected)
            {
                _controlSocket.Connect(Address, 21);
                if (_controlSocket.Connected)
                {
                    _reader = new StreamReader(_controlSocket.GetStream());
                    _writer = new StreamWriter(_controlSocket.GetStream()) { AutoFlush = true };
                    StringBuilder sb = new StringBuilder();
                    string Response = _reader.ReadLine();
                    if (Response.StartsWith("220-"))
                    {
                        while (true)
                        {
                            Response = _reader.ReadLine();
                            if (Response.StartsWith("220 "))
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        public void Login()
        {
            string Command = string.Format("USER {0}", User);
            _writer.WriteLine(Command);
            string Response = _reader.ReadLine();
            if (Response.StartsWith("331 "))
            {
                Command = string.Format("PASS {0}", Password);
                _writer.WriteLine(Command);
                Response = _reader.ReadLine();
                LoggedOn = true;
            }
        }
        public void Logout()
        {
            if (_controlSocket.Connected && LoggedOn)
            {
                string Command = "QUIT";
                _writer.WriteLine(Command);
                string Response = _reader.ReadLine();
                if (Response.StartsWith("221 "))
                {
                    LoggedOn = false;
                    _controlSocket.Close();
                }
            }
        }
        public void RemoveDirectory(string dir)
        {
            string Command = string.Format("RMD {0}", dir);
            _writer.WriteLine(Command);
            string Response = _reader.ReadLine();
        }
        public void CreateDirectory(string dir)
        {
            string Command = string.Format("MKD {0}", dir);
            _writer.WriteLine(Command);
            string Response = _reader.ReadLine();
        }
        public void ChangeDirectory(string dir)
        {
            string Command = string.Format("CWD {0}", dir);
            _writer.WriteLine(Command);
            string Response = _reader.ReadLine();
        }
        public void CurrentDirectory()
        {
            string Command = "PWD";
            _writer.WriteLine(Command);
            string Response = _reader.ReadLine();
        }
        public void Upload(string filename)
        {
            string Command = "PASV";
            _writer.WriteLine(Command);
            string Response = _reader.ReadLine();
            if (Response.StartsWith("227 "))
            {
                IPEndPoint server_data_endpoint = GetServerEndpoint(Response);
                string remoteFileName = Path.GetFileName(filename);
                Command = string.Format("STOR {0}", remoteFileName);
                _writer.WriteLine(Command);
                TcpClient data_channel = new TcpClient();
                data_channel.Connect(server_data_endpoint);
                Response = _reader.ReadLine();
                if (Response.StartsWith("150 "))
                {
                    NetworkStream ns = data_channel.GetStream();
                    int blocksize = 1024;
                    byte[] buffer = new byte[blocksize];
                    int byteread = 0;
                    lock (this)
                    {
                        FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                        while (true)
                        {
                            byteread = fs.Read(buffer, 0, blocksize);
                            ns.Write(buffer, 0, byteread);
                            if (byteread == 0)
                            {
                                break;
                            }
                        }
                        ns.Flush();
                        ns.Close();
                    }
                    Response = _reader.ReadLine();
                    if (Response.StartsWith("226 "))
                    {
                        data_channel.Close();
                    }
                }
                data_channel.Close();
            }
        }

        public event Action<string, string> FileDownloaded;

        public void DownloadFolder(string remoteFilePath, string localFilePath)
        {
            if (IsDirectory(remoteFilePath))
            {
                // 1. Tạo thư mục cục bộ nếu nó không tồn tại.
                if (!Directory.Exists(localFilePath))
                {
                    Directory.CreateDirectory(localFilePath);
                }

                // 2. Lấy danh sách các tệp và thư mục con trong thư mục trên máy chủ FTP.
                List<string> fileList = ListFilesAndFolders(remoteFilePath);
                if (fileList.Count() == 0) return;
                // 3. Tải tất cả các tệp con trong thư mục.
                for (int i = 0; i < fileList.Count(); i++)
                {
                    string remoteItemPath = remoteFilePath + "\\" + fileList[i];
                    string localItemPath = localFilePath + "\\" + fileList[i];
                    DownloadFolder(remoteItemPath, localItemPath);
                }
            }
            else
            {
                OnFileDownloaded(remoteFilePath, localFilePath);
            }
        }

        protected virtual void OnFileDownloaded(string remoteFile, string localFile)
        {
            FileDownloaded?.Invoke(remoteFile, localFile);
        }

        public List<string> ListFilesAndFolders(string folderPath)
        {
            string Command = string.Format("CWD {0}", "/");
            _writer.WriteLine(Command);
            string Response = _reader.ReadLine();
            if (Response.StartsWith("250 "))
            {
                Command = "PASV";
                _writer.WriteLine(Command);
                Response = _reader.ReadLine();
                if (Response.StartsWith("227 "))
                {
                    IPEndPoint server_data_endpoint = GetServerEndpoint(Response);
                    Command = server_data_endpoint.ToString();
                    TcpClient data_channel = new TcpClient();
                    data_channel.Connect(server_data_endpoint);

                    Command = string.Format("NLST {0}", folderPath.Replace("\\", "/"));
                    _writer.WriteLine(Command);
                    Response = _reader.ReadLine();
                    if (Response.StartsWith("150 "))
                    {
                        List<string> fileList = new List<string>();
                        StreamReader sr = new StreamReader(data_channel.GetStream());
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            line = line.Split('/').Last();
                            fileList.Add(line);
                        }
                        Response = _reader.ReadLine();
                        if (Response.StartsWith("226 "))
                        {
                            data_channel.Close();
                        }

                        return fileList;
                    }
                }
            }
            return null;
        }

        public bool IsDirectory(string path)
        {
            string Command = string.Format("CWD {0}", "/");
            _writer.WriteLine(Command);
            string Response = _reader.ReadLine();
            if (Response.StartsWith("250 "))
            {
                // Gửi lệnh CWD để thay đổi đến đường dẫn cụ thể
                Command = string.Format("CWD {0}", path.Replace("\\", "/"));
                _writer.WriteLine(Command);
                Response = _reader.ReadLine();

                if (Response.StartsWith("250 "))
                {
                    // Nếu thay đổi thành công, đường dẫn đó là một thư mục
                    return true;
                }

            }
            return false;
        }


        public void Download(string remoteFolderPath, string remoteFilePath, string localFilePath)
        {
            string Command = string.Format("CWD {0}", remoteFolderPath.Replace("\\", "/"));
            _writer.WriteLine(Command);
            string Response = _reader.ReadLine();
            if (Response.StartsWith("250 "))
            {
                Command = "PASV";
                _writer.WriteLine(Command);
                Response = _reader.ReadLine();
                if (Response.StartsWith("227 "))
                {
                    IPEndPoint server_data_endpoint = GetServerEndpoint(Response);
                    Command = server_data_endpoint.ToString();
                    TcpClient data_channel = new TcpClient();
                    data_channel.Connect(server_data_endpoint);

                    Command = string.Format("RETR {0}", remoteFilePath);
                    _writer.WriteLine(Command);
                    Response = _reader.ReadLine();
                    if (Response.StartsWith("150 "))
                    {
                        NetworkStream ns = data_channel.GetStream();
                        int blocksize = 1024;
                        byte[] buffer = new byte[blocksize];
                        int byteread = 0;
                        lock (this)
                        {
                            FileStream fs = new FileStream(localFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                            while (true)
                            {
                                byteread = ns.Read(buffer, 0, blocksize);
                                fs.Write(buffer, 0, byteread);
                                if (byteread == 0)
                                {
                                    break;
                                }
                            }
                            fs.Flush();
                            fs.Close();
                        }
                        Response = _reader.ReadLine();
                        if (Response.StartsWith("226 "))
                        {
                            data_channel.Close();
                        }
                    }
                }
            }
        }
        public void List()
        {
            string Command = "PASV";
            _writer.WriteLine(Command);
            string Response = _reader.ReadLine();
            if (Response.StartsWith("227 "))
            {
                IPEndPoint server_data_endpoint = GetServerEndpoint(Response);
                Command = server_data_endpoint.ToString();
                TcpClient data_channel = new TcpClient();
                data_channel.Connect(server_data_endpoint);

                Command = "LIST";
                _writer.WriteLine(Command);
                Response = _reader.ReadLine();
                if (Response.StartsWith("150 "))
                {

                    StreamReader sr = new StreamReader(data_channel.GetStream());
                    Response = sr.ReadToEnd();
                    Response = _reader.ReadLine();
                    if (Response.StartsWith("226 "))
                    {
                        data_channel.Close();
                    }
                }
            }
        }
        IPEndPoint GetServerEndpoint(string response)
        {
            int start = response.IndexOf('(');
            int end = response.IndexOf(')');
            string substr = response.Substring(start + 1, end - start - 1);
            string[] octets = substr.Split(',');
            int port = int.Parse(octets[4]) * 256 + int.Parse(octets[5]);
            IPAddress address = new IPAddress(new byte[] { byte.Parse(octets[0]), byte.Parse(octets[1]), byte.Parse(octets[2]), byte.Parse(octets[3]) });
            return new IPEndPoint(address, port);
        }
        #endregion
    }
}
