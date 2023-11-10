using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;

namespace FTPServer
{
    public class FTPServer
    {
        int a, b;
        TcpListener _controlSocket;
        StreamWriter _writer;
        StreamReader _reader;
        int _sessionID = 2;
        readonly BindingList<string> _responses = new BindingList<string>();
        readonly BindingList<string> _commands = new BindingList<string>();
        readonly string _rootPath = @"E:/LEARN-04/PBL4/FileServer";

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
        public bool isRunning;
        public void Start()
        {
            Address = IPAddress.Parse("127.0.0.1");
            User = "user";
            Password = "mxt@3132003";
            randomPort();
            _responses.Clear();
            _commands.Clear();
            _controlSocket = new TcpListener(Address, 21);
            _responses.ListChanged -= Responses_ListChanged;
            _responses.ListChanged += Responses_ListChanged;
            _commands.ListChanged -= Commands_ListChanged;
            _commands.ListChanged += Commands_ListChanged;
            isRunning = true;
            try
            {
                _controlSocket.Start();
            }
            catch
            {
                Command = "Lỗi Start Server!!! Vui lòng kiểm tra lại";
                return;
            }
            Console.WriteLine("FTP Server is running...");
            while (isRunning)
            {
                TcpClient client = _controlSocket.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }

        public void Stop()
        {
            isRunning = false;
            _controlSocket.Stop();
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            _reader = new StreamReader(client.GetStream());
            _writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
            IPAddress IPClient = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            TcpListener data_listener = new TcpListener(a * 256 + b);
            TcpClient data_channel = new TcpClient();
            string _command;
            int sessionID = GetSessionID();
            string nameSission = "FTP Session " + sessionID + " " + IPClient.ToString() + " ";
            _command = "220-Welcome to FTP Server";
            Command = nameSission + _command;
            _writer.WriteLine(_command);
            _command = "220 FTP Server MXT";
            Command = nameSission + _command;
            _writer.WriteLine(_command);
            
            string username = null;
            string password = null;
            bool isLoggedIn = false;
            string currentFilePath = _rootPath;
            try
            {
                while (true)
                {
                    if (!client.Connected)
                        break;

                    string request = _reader.ReadLine();
                    if (request == null)
                        break;

                    Response = nameSission + request;

                    string[] parts = request.Split(' ');
                    string command = parts[0].ToUpperInvariant();
                    if (parts.Length >= 2) parts[1] = string.Join(" ", parts, 1, parts.Length - 1);

                    if (isLoggedIn) //Logged in successfully
                    {
                        if (command == "USER")
                        {
                            _command = "503 Already logged in. QUIT first.";
                            Command = nameSission + _command;
                            _writer.WriteLine(_command);
                        }
                        else if (command == "QUIT")
                        {
                            _command = "221 Goodbye";
                            Command = nameSission + _command;
                            _writer.WriteLine(_command);
                            isLoggedIn = false;
                            break;
                        }
                        else if(command == "PASV")
                        {
                            string[] ipAddressParts = Address.ToString().Split('.');
                            string IP = string.Join(",", ipAddressParts);
                            reNewPort(a, b);
                            _command = "227 Entering Passive Mode (" + IP + "," + a + "," + b + ")";
                            Command = nameSission + _command;
                            data_listener = new TcpListener(Address, a * 256 + b);
                            try
                            {
                                data_listener.Start();
                            }
                            catch
                            {
                                Command = "Lỡ bị lỗi";
                            }
                            _writer.WriteLine(_command);
                            data_channel = data_listener.AcceptTcpClient();
                            Console.WriteLine("OK");
                        }
                        else if(command == "CWD")
                        {
                            string path;
                            if (parts[1] == "/")
                            {
                                path = _rootPath;
                            }
                            else
                            {
                                path = Path.GetFullPath(Path.Combine(currentFilePath, parts[1]));
                                path = path.Replace("\\", "/");
                            }
                            if (parts[1] == "")
                            {
                                _command = "501 Missing required argument";
                            }
                            else if(!path.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
                            {
                                _command = "550 Couldn't open the file or directory";
                            }
                            else if(Directory.Exists(path))
                            {
                                _command = "250 CWD command successful";
                                currentFilePath = path;
                            } 
                            else
                            {
                                _command = "550 Couldn't open the file or directory";
                            }
                            Command = nameSission + _command;
                            _writer.WriteLine(_command);
                        }
                        else if(command == "NLST")
                        {
                            string path = Path.GetFullPath(Path.Combine(_rootPath, parts[1]));
                            path = path.Replace("\\", "/");
                            if (Directory.Exists(path) && data_channel.Connected)
                            {
                                _command = "150 About to start data transfer.";
                                Command = nameSission + _command;
                                _writer.WriteLine(_command);

                                List<string> fileAndDirectoryNames = new List<string>();
                                DirectoryInfo parentDirectory = new DirectoryInfo(path);
                                DirectoryInfo[] subDirectories = parentDirectory.GetDirectories();
                                FileInfo[] files = parentDirectory.GetFiles();

                                foreach (var subDir in subDirectories)
                                {
                                    fileAndDirectoryNames.Add(subDir.FullName.Replace("\\", "/"));
                                }

                                foreach (var file in files)
                                {
                                    fileAndDirectoryNames.Add(file.FullName.Replace("\\", "/"));
                                }

                                StreamWriter sw = new StreamWriter(data_channel.GetStream());

                                foreach (string item in fileAndDirectoryNames)
                                {
                                    sw.WriteLine(item);
                                }

                                _command = "226 Operation successful";
                                Command = nameSission + _command;
                                _writer.WriteLine(_command);

                                sw.Close();
                                data_channel.Close();
                                data_listener.Stop();
                            }
                            else
                            {
                                _command = "550 Couldn't open the file or directory";
                                Command = nameSission + _command;
                                _writer.WriteLine(_command);
                            }
                        }
                        else if(command == "RETR")
                        {
                            string path = Path.GetFullPath(Path.Combine(_rootPath, parts[1]));
                            path = path.Replace("\\", "/");
                            if (Directory.Exists(path) && data_channel.Connected)
                            {
                                _command = "150 About to start data transfer.";
                                Command = nameSission + _command;
                                _writer.WriteLine(_command);

                                NetworkStream ns = data_channel.GetStream();
                                int blocksize = 1024;
                                byte[] buffer = new byte[blocksize];
                                int byteread = 0;
                                lock (this)
                                {
                                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
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

                                _command = "226 Operation successful";
                                Command = nameSission + _command;
                                _writer.WriteLine(_command);

                                data_channel.Close();
                                data_listener.Stop();
                            }
                            else
                            {
                                _command = "550 Couldn't open the file or directory";
                                Command = nameSission + _command;
                                _writer.WriteLine(_command);
                            }
                        }
                    }
                    else //Login failed
                    {
                        if (command == "USER")
                        {
                            _command = "331 User name okay, need password.";
                            username = parts.Length > 1 ? parts[1] : null;
                            Command = nameSission + _command;
                            _writer.WriteLine(_command);
                        }
                        else if (command == "PASS")
                        {
                            if (username == User && parts.Length > 1 && parts[1] == Password)
                            {
                                _command = "230 User logged in, proceed.";
                                isLoggedIn = true;
                                Command = nameSission + _command;
                                _writer.WriteLine(_command);
                            }
                            else
                            {
                                _command = "530 Not logged in.";
                                Command = nameSission + _command;
                                _writer.WriteLine(_command);
                            }
                        }
                        else
                        {
                            _command = "530 Please log in with USER and PASS first.";
                            Command = nameSission + _command;
                            _writer.WriteLine(_command);
                        }
                    }
                    // Add more FTP commands and functionality here...
                }
            } catch (IOException ex)
            {

            } finally
            {
                client.Close();
            }
            
        }

        public void randomPort()
        {
            Random random = new Random();
            a = random.Next(4, 256);
            b = random.Next(0, 256);
        }

        public void reNewPort(int a, int b)
        {
            b += 2;
            if (b > 255) a += 1;
            if (a > 255)
            {
                a = 4;
                if (b % 2 == 0) b = 1;
                else b = 0;
            }
            TcpListener listener = new TcpListener(Address, a * 256 + b);
            try
            {
                listener.Start();
            }
            catch (SocketException)
            {
                reNewPort(a, b);
            }
            finally
            {
                listener.Stop();
            }
        }

        private int GetSessionID()
        {
            return _sessionID++;
        }

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
    }
}