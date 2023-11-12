using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPClient
{
    public class TaskSession
    {
        public string type;
        public string localPath;
        public string remotePath;

        public TaskSession(string type, string localPath, string remotePath)
        {
            this.type = type;
            this.localPath = localPath;
            this.remotePath = remotePath;
        }
    }
}
