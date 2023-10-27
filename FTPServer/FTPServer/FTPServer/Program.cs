using System;
using System.Net;
namespace FTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            FTPServer server = new FTPServer();
            server.ResponseListChangedHandler = s => Console.WriteLine("C> {0}", s);
            server.CommandListChangedHandler = s => Console.WriteLine("S> {0}", s);
            server.Start();
        }
    }
}