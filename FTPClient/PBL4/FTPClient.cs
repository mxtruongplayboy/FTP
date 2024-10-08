﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace FtpClient
{
    public class FtpClient
    {
        #region Properties
        TcpClient _controlSocket;
        StreamWriter _writer;
        StreamReader _reader;
        readonly BindingList<string> _responses = new BindingList<string>();
        readonly BindingList<string> _commands = new BindingList<string>();
        public string Command
        {
            get => _commands.Last();
            private set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _commands.Add(value);
                }
            }
        }
        public string Response
        {
            get => _responses.Last();
            private set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _responses.Add(value);
                }
            }
        }
        public IPAddress Address { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool Connected => _controlSocket.Connected;
        public bool LoggedOn { get; private set; }
        #endregion
        #region Commands  
        public void Connect()
        {
            _responses.Clear();
            _commands.Clear();
            _controlSocket = new TcpClient();
            if (!_controlSocket.Connected)
            {
                _controlSocket.Connect(Address, 21);
                if (_controlSocket.Connected)
                {
                    _responses.ListChanged -= Responses_ListChanged;
                    _responses.ListChanged += Responses_ListChanged;
                    _commands.ListChanged -= Commands_ListChanged;
                    _commands.ListChanged += Commands_ListChanged;
                    _reader = new StreamReader(_controlSocket.GetStream());
                    _writer = new StreamWriter(_controlSocket.GetStream()) { AutoFlush = true };
                    StringBuilder sb = new StringBuilder();
                    Response = _reader.ReadLine();
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
            Command = string.Format("USER {0}", User);
            _writer.WriteLine(Command);
            Response = _reader.ReadLine();
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
                Command = "QUIT";
                _writer.WriteLine(Command);
                Response = _reader.ReadLine();
                if (Response.StartsWith("221 "))
                {
                    LoggedOn = false;
                    _controlSocket.Close();
                }
            }
        }
        public void RemoveDirectory(string dir)
        {
            Command = string.Format("RMD {0}", dir);
            _writer.WriteLine(Command);
            Response = _reader.ReadLine();
        }
        public void CreateDirectory(string dir)
        {
            Command = string.Format("MKD {0}", dir);
            _writer.WriteLine(Command);
            Response = _reader.ReadLine();
        }
        public void ChangeDirectory(string dir)
        {
            Command = string.Format("CWD {0}", dir);
            _writer.WriteLine(Command);
            Response = _reader.ReadLine();
        }
        public void CurrentDirectory()
        {
            Command = "PWD";
            _writer.WriteLine(Command);
            Response = _reader.ReadLine();
        }
        public void Upload(string filename)
        {
            Command = "PASV";
            _writer.WriteLine(Command);
            Response = _reader.ReadLine();
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

        public void DownloadFolder(string remoteFilePath, string localFilePath)
        {
            if(IsDirectory(remoteFilePath))
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
                Download(Path.GetDirectoryName(remoteFilePath), Path.GetFileName(remoteFilePath), localFilePath);
            }
        }

        public List<string> ListFilesAndFolders(string folderPath)
        {
            Command = "PASV";
            _writer.WriteLine(Command);
            Response = _reader.ReadLine();
            if(Response.StartsWith("227 "))
            {
                IPEndPoint server_data_endpoint = GetServerEndpoint(Response);
                Command = string.Format("CWD {0}", "/");
                _writer.WriteLine(Command);
                Response = _reader.ReadLine();
                if (Response.StartsWith("250 "))
                {
                    Command = string.Format("NLST {0}", folderPath.Replace("\\", "/"));
                    _writer.WriteLine(Command);
                    TcpClient data_channel = new TcpClient();
                    data_channel.Connect(server_data_endpoint);
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
            Command = string.Format("CWD {0}", "/");
            _writer.WriteLine(Command);
            Response = _reader.ReadLine();
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
            Command = "PASV";
            _writer.WriteLine(Command);
            Response = _reader.ReadLine();
            if (Response.StartsWith("227 "))
            {
                IPEndPoint server_data_endpoint = GetServerEndpoint(Response);
                Command = string.Format("CWD {0}", remoteFolderPath.Replace("\\","/"));
                _writer.WriteLine(Command);
                Response = _reader.ReadLine();
                if (Response.StartsWith("250 "))
                {
                    Command = string.Format("RETR {0}", remoteFilePath);
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
            Command = "PASV";
            _writer.WriteLine(Command);
            Response = _reader.ReadLine();
            if (Response.StartsWith("227 "))
            {
                IPEndPoint server_data_endpoint = GetServerEndpoint(Response);
                Command = "LIST";
                _writer.WriteLine(Command);
                TcpClient data_channel = new TcpClient();
                data_channel.Connect(server_data_endpoint);
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
        #region Logging
        public Action<string> ResponseListChangedHandler { get; set; }
        public Action<string> CommandListChangedHandler { get; set; }
        private void Responses_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                string response = _responses[e.NewIndex];
                ResponseListChangedHandler?.Invoke(response);
            }
        }
        private void Commands_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                string command = _commands[e.NewIndex];
                CommandListChangedHandler?.Invoke(command);
            }
        }
        #endregion
    }
}