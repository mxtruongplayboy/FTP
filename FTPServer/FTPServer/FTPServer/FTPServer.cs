using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.IO;
using System.Reflection;

namespace FTPServer
{
    public class FTPServer
    {
        int a, b;
        TcpListener _controlSocket;
        int _sessionID = 2;
        readonly string _rootPath = @"E:/LEARN-04/PBL4/FileServer";
        private object lockObject = new object();

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
            _controlSocket = new TcpListener(Address, 21);
            isRunning = true;
            try
            {
                _controlSocket.Start();
            }
            catch
            {
                CommandStatus("Server", "Lỗi Start Server!!! Vui lòng kiểm tra lại");
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
            StreamReader _reader = new StreamReader(client.GetStream());
            StreamWriter _writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
            IPAddress IPClient = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            TcpListener data_listener = new TcpListener(0);
            string _command;
            string _respond;
            int sessionID = GetSessionID();
            string nameSission = "FTP Session " + sessionID + " " + IPClient.ToString() + " ";
            _command = "220-Welcome to FTP Server";
            CommandStatus(nameSission, _command);
            _writer.WriteLine(_command);
            _command = "220 FTP Server MXT";
            CommandStatus(nameSission, _command);
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

                    ResponseStatus(nameSission, request);

                    string[] parts = request.Split(' ');
                    string command = parts[0].ToUpperInvariant();
                    if (parts.Length >= 2) parts[1] = string.Join(" ", parts, 1, parts.Length - 1);

                    if (isLoggedIn) //Logged in successfully
                    {
                        if (command == "USER")
                        {
                            _command = "503 Already logged in. QUIT first.";
                            CommandStatus(nameSission, _command);
                            _writer.WriteLine(_command);
                        }
                        else if (command == "QUIT")
                        {
                            _command = "221 Goodbye";
                            CommandStatus(nameSission, _command);
                            _writer.WriteLine(_command);
                            isLoggedIn = false;
                            break;
                        }
                        else if(command == "PASV")
                        {
                            string[] ipAddressParts = Address.ToString().Split('.');
                            string IP = string.Join(",", ipAddressParts);
                            reNewPort();
                            _command = "227 Entering Passive Mode (" + IP + "," + a + "," + b + ")";
                            CommandStatus(nameSission, _command);
                            data_listener = new TcpListener(Address, a * 256 + b);
                            try
                            {
                                data_listener.Start();
                            }
                            catch
                            {
                                reNewPort();
                                _command = "227 Entering Passive Mode (" + IP + "," + a + "," + b + ")";
                                CommandStatus(nameSission, _command);
                                data_listener = new TcpListener(Address, a * 256 + b);
                            }
                            _writer.WriteLine(_command);
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
                                path = Path.GetFullPath(Path.Combine(_rootPath, parts[1]));
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
                            CommandStatus(nameSission, _command);
                            _writer.WriteLine(_command);
                        }
                        else if(command == "NLST")
                        {
                            TcpClient data_channel = data_listener.AcceptTcpClient();
                            string path = Path.GetFullPath(Path.Combine(currentFilePath, parts[1]));
                            path = path.Replace("\\", "/");
                            if (Directory.Exists(path) && data_channel.Connected)
                            {
                                _command = "150 About to start data transfer.";
                                CommandStatus(nameSission, _command);
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
                                CommandStatus(nameSission, _command);
                                _writer.WriteLine(_command);

                                sw.Close();
                                data_channel.Close();
                                data_listener.Stop();
                            }
                            else
                            {
                                _command = "550 Couldn't open the file or directory";
                                CommandStatus(nameSission, _command);
                                _writer.WriteLine(_command);
                            }
                        }
                        else if(command == "RETR")
                        {
                            string path = Path.GetFullPath(Path.Combine(currentFilePath, parts[1]));
                            path = path.Replace("\\", "/");
                            if(File.Exists(path))
                            {
                                _command = "150 About to start data transfer.";
                                CommandStatus(nameSission, _command);
                                _writer.WriteLine(_command);

                                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                                {
                                    long length = fs.Length;
                                    _command = $"{length}";
                                    CommandStatus(nameSission, _command);
                                    _writer.WriteLine(_command);

                                    long block = (int)Math.Pow(2, 20) * 256;
                                    int numThread = (int)Math.Ceiling((double)length / block);
                                    int numThreadNow = 0;
                                    while(numThreadNow < numThread)
                                    {
                                        TcpClient data_channel = data_listener.AcceptTcpClient();
                                        long num = numThreadNow;
                                        Thread thread = new Thread(() => HandleTransfer(data_channel, path, num * block, block));
                                        numThreadNow++;
                                        thread.Start();
                                    }
                                }
                                _respond = _reader.ReadLine();
                                ResponseStatus(nameSission, _respond);

                                _command = "226 Operation successful";
                                CommandStatus(nameSission, _command);
                                _writer.WriteLine(_command);                             
                            }
                            else
                            {
                                _command = "550 Couldn't open the file or directory";
                                CommandStatus(nameSission, _command);
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
                            CommandStatus(nameSission, _command);
                            _writer.WriteLine(_command);
                        }
                        else if (command == "PASS")
                        {
                            if (username == User && parts.Length > 1 && parts[1] == Password)
                            {
                                _command = "230 User logged in, proceed.";
                                isLoggedIn = true;
                                CommandStatus(nameSission, _command);
                                _writer.WriteLine(_command);
                            }
                            else
                            {
                                _command = "530 Not logged in.";
                                CommandStatus(nameSission, _command);
                                _writer.WriteLine(_command);
                            }
                        }
                        else
                        {
                            _command = "530 Please log in with USER and PASS first.";
                            CommandStatus(nameSission, _command);
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

        private void HandleTransfer(TcpClient data_channel,string fileName, long offset, long length)
        {
            StreamReader _reader = new StreamReader(data_channel.GetStream());
            StreamWriter _writer = new StreamWriter(data_channel.GetStream()) { AutoFlush = true };
            try
            {
                NetworkStream ns = data_channel.GetStream();
                int blocksize = 1024;
                byte[] buffer = new byte[blocksize];
                int byteread = 0;
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                fs.Seek(offset, SeekOrigin.Begin);
                long i = 0;
                while (i < length)
                {
                    byteread = fs.Read(buffer, 0, (int)Math.Min(blocksize, length - i));
                    ns.Write(buffer, 0, byteread);
                    if (byteread == 0)
                    {
                        break;
                    }
                    i += byteread;
                }
                ns.Close();
                fs.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void randomPort()
        {
            Random random = new Random();
            this.a = random.Next(4, 256);
            this.b = random.Next(0, 256);
        }

        public void reNewPort()
        {
           lock (lockObject)
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
                    reNewPort();
                }
                finally
                {
                    listener.Stop();
                }
            }
        }

        private int GetSessionID()
        {
            lock (lockObject)
            {
                return _sessionID++;
            }
        }

        private void CommandStatus(string sessionId, string message)
        {
            lock(lockObject)
            {
                DateTime now = DateTime.Now;
                Console.WriteLine($"S> {now}\tSession {sessionId}\t {message} \n");
            }
        }

        private void ResponseStatus(string sessionId, string message)
        {
            lock (lockObject)
            {
                DateTime now = DateTime.Now;
                Console.WriteLine($"C> {now}\tSession {sessionId}\t {message} \n");
            }
        }
    }
}