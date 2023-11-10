using FTPClient;
using System;
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
        FTPSession _mainSession;
        FTPSession _subSession1;
        FTPSession _subSession2;
        readonly BindingList<string> _responses = new BindingList<string>();
        readonly BindingList<string> _commands = new BindingList<string>();

        public IPAddress Address { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool Connected => _mainSession.Connected;
        public bool LoggedOn { get; private set; }
        #endregion
        #region Commands  
        public void Connect()
        {
            _responses.Clear();
            _commands.Clear();
            _mainSession = new FTPSession(Address,User,Password,_responses, _commands);
            _subSession1 = new FTPSession(Address, User, Password, _responses, _commands);
            _subSession2 = new FTPSession(Address, User, Password, _responses, _commands);
            if (!_mainSession.Connected)
            {
                _responses.ListChanged -= Responses_ListChanged;
                _responses.ListChanged += Responses_ListChanged;
                _commands.ListChanged -= Commands_ListChanged;
                _commands.ListChanged += Commands_ListChanged;
                _mainSession.Connect();
                _mainSession.Login();
            }
        }

        public void Download(string remoteFilePath, string localFilePath)
        {
            
        }
        #endregion Commands
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