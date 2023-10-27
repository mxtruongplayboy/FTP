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
        TcpListener _controlSocket;
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
        public bool isRunning;
        public void Start()
        {
            Address = IPAddress.Parse("192.168.252.118");
            User = "user";
            Password = "mxt@3132003";
            _responses.Clear();
            _commands.Clear();
            _controlSocket = new TcpListener(Address, 21);
            _responses.ListChanged -= Responses_ListChanged;
            _responses.ListChanged += Responses_ListChanged;
            _commands.ListChanged -= Commands_ListChanged;
            _commands.ListChanged += Commands_ListChanged;
            isRunning = true;
            _controlSocket.Start();
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
            Console.WriteLine(IPClient.ToString());
            _writer.WriteLine("220-Welcome to FTP Server");
            _writer.WriteLine("220 FTP Server MXT");

            string username = null;
            string password = null;
            bool isLoggedIn = false;
            try
            {
                while (true)
                {
                    if (!client.Connected)
                        break;

                    string request = _reader.ReadLine();
                    if (request == null)
                        break;

                    Response = request;

                    string[] parts = request.Split(' ');
                    string command = parts[0].ToUpperInvariant();

                    if (command == "USER" && isLoggedIn == false)
                    {
                        Command = "331 User name okay, need password.";
                        username = parts.Length > 1 ? parts[1] : null;
                        _writer.WriteLine(Command);
                    }
                    else if(command == "USER" && isLoggedIn)
                    {
                        Command = "503 Already logged in. QUIT first.";
                        _writer.WriteLine(Command);
                    }
                    else if (command == "PASS")
                    {
                        if (username == User && parts.Length > 1 && parts[1] == Password)
                        {
                            Command = "230 User logged in, proceed.";
                            isLoggedIn = true;
                            _writer.WriteLine(Command);
                        }
                        else
                        {
                            Command = "530 Not logged in.";
                            _writer.WriteLine(Command);
                        }
                    } else if(command == "QUIT" && isLoggedIn)
                    {
                        Command = "221 Goodbye";
                        _writer.WriteLine(Command);
                        isLoggedIn = false;
                        break;
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