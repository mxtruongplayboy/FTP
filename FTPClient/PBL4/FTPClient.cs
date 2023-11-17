using FTPClient;
using System.Net;

namespace FtpClient
{
    public class FtpClient
    {
        #region Properties
        FTPSession _mainSession;
        FTPSession _subSession1;
        FTPSession _subSession2;
        private object lockObject = new object();
        List<TaskSession> _taskMain = new List<TaskSession>();
        List<TaskSession> _taskSub = new List<TaskSession>();
        bool check1 = true;
        int a;

        public IPAddress Address { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool Connected => _mainSession.Connected;
        public bool LoggedOn { get; private set; }
        #endregion
        #region Commands  
        public void Connect()
        {
            _mainSession = new FTPSession(Address, User, Password);
            _mainSession.FileDownloaded += HandleFileDownloaded;

            _subSession1 = new FTPSession(Address, User, Password);
            _subSession2 = new FTPSession(Address, User, Password);
            _mainSession.Connect();
            _mainSession.Login();
        }

        public void Download(string remoteFilePath, string localFilePath)
        {
            TaskSession task = new TaskSession("NLST",localFilePath, remoteFilePath);
            lock(lockObject)
            {
                _taskMain.Add(task);
            }
            if(_mainSession.status) TaskMainProcess(_mainSession);
        }

        public void TaskMainProcess(FTPSession mainSession)
        {
            Thread sessionThread = new Thread(new ParameterizedThreadStart(HandleSessionMain));
            sessionThread.Start(mainSession);
        }

        private void HandleSessionMain(object obj)
        {
            FTPSession mainSession = (FTPSession)obj;
            mainSession.status = false;
            while(_taskMain.Count() > 0)
            {
                TaskSession task = _taskMain[0];
                if(task != null)
                {
                    _taskMain.RemoveAt(0);
                    if (task.type == "NLST")
                    {
                        _mainSession.DownloadFolder(task.remotePath, task.localPath);
                    }
                }
            }
            mainSession.status = true;
        }

        private void HandleFileDownloaded(string remoteFilePath, string localFilePath)
        {
            _taskSub.Add(new("Download", localFilePath, remoteFilePath));
            if (_subSession1.status)
            {
                _subSession1.status = false;
                if (!_subSession1.Connected)
                {
                    _subSession1.Connect();
                    _subSession1.Login();
                }
                Thread sessionThread = new Thread(new ParameterizedThreadStart(HandleSessionSub1));
                sessionThread.Start(_subSession1);
            }
            if(_subSession2.status)
            {
                _subSession2.status = false;
                if (!_subSession2.Connected)
                {
                    _subSession2.Connect();
                    _subSession2.Login();
                }
                Thread sessionThread = new Thread(new ParameterizedThreadStart(HandleSessionSub2));
                sessionThread.Start(_subSession2);
            }
        }

        private void HandleSessionSub1(object obj)
        {
            FTPSession subSession = (FTPSession)obj;
            while (_taskSub.Count() > 0)
            {
                TaskSession task = GetTaskSession();
                if(task != null)
                {
                    if (task.type == "Download")
                    {
                        subSession.Download(Path.GetDirectoryName(task.remotePath), Path.GetFileName(task.remotePath), task.localPath);
                    }
                }
            }
            _subSession1.status = true;
        }

        private void HandleSessionSub2(object obj)
        {
            FTPSession subSession = (FTPSession)obj;
            while (_taskSub.Count() > 0)
            {
                TaskSession task = GetTaskSession();
                if (task != null)
                {
                    if (task.type == "Download")
                    {
                        subSession.Download(Path.GetDirectoryName(task.remotePath), Path.GetFileName(task.remotePath), task.localPath);
                    }
                }
            }
            _subSession2.status = true;
        }

        public TaskSession GetTaskSession()
        {
            lock (lockObject)
            {
                if (_taskSub.Count > 0)
                {
                    TaskSession task = _taskSub[0];
                    _taskSub.RemoveAt(0);
                    return task;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion Commands
    }
}