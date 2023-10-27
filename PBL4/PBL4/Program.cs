using System;
using System.Net;
namespace FtpClient
{
    class Program
    {
        static void Help()
        {
            Console.WriteLine("Supported commands");
            Console.WriteLine("Account\t\tConnect\t\tLogin\t\tLogout\t\tQuit");
            Console.WriteLine("Dir\t\tMkDir\t\tRmDir\t\tCd\t\tCr");
            Console.WriteLine("Upload\t\tDownload");
            Console.WriteLine("--------------------------------------------");
        }
        static void Reset(ref FtpClient client)
        {
            client = new FtpClient
            {
                ResponseListChangedHandler = s => { Console.WriteLine("S> {0}", s); },
                CommandListChangedHandler = s => Console.WriteLine("C> {0}", s)
            };
            Console.Write("User: ");
            client.User = Console.ReadLine();
            Console.Write("Password: ");
            client.Password = Console.ReadLine();
            Console.Write("IP: ");
            client.Address = IPAddress.Parse(Console.ReadLine());
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Simple FTP client by MYPE. Type Help to get supported commands.");
            FtpClient client = new FtpClient();
            bool quit = false;
            string fileRemote, fileLocal, folder;
            while (!quit)
            {
                Console.Write("fpt> ");
                string cmd = Console.ReadLine();
                try
                {
                    switch (cmd.ToUpper())
                    {
                        case "ACCOUNT":
                            Reset(ref client);
                            break;
                        case "QUIT":
                            quit = true;
                            break;
                        case "HELP":
                            Help();
                            break;
                        case "CONNECT":
                            client.Connect();
                            break;
                        case "LOGIN":
                            client.Login();
                            break;
                        case "LOGOUT":
                            client.Logout();
                            break;
                        case "CR":
                            client.CurrentDirectory();
                            break;
                        case "CD":
                            Console.Write(">Go to folder: ");
                            folder = Console.ReadLine();
                            client.ChangeDirectory(folder);
                            break;
                        case "MKDIR":
                            Console.Write(">New folder name: ");
                            folder = Console.ReadLine();
                            client.CreateDirectory(folder);
                            break;
                        case "RMDIR":
                            Console.Write(">Folder to remove: ");
                            folder = Console.ReadLine();
                            client.RemoveDirectory(folder);
                            break;
                        case "DIR":
                            client.List();
                            break;
                        case "DOWNLOAD":
                            Console.Write(">File name remote: ");
                            fileRemote = Console.ReadLine();
                            Console.Write(">File name local: ");
                            fileLocal = Console.ReadLine();
                            client.DownloadFolder(fileRemote, fileLocal);
                            break;
                        case "UPLOAD":
                            //Console.Write(">File name: ");
                            //file = Console.ReadLine();
                            //client.Upload(file);
                            break;
                        default:
                            Console.WriteLine("Unknown command");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}