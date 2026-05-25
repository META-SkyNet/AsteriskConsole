using System;
using System.IO;
using Renci.SshNet;

namespace VisualAsterisk.Core.SSH
{
    public class ScpCommand
    {
        private ScpClient? _client;
        private string _host = string.Empty;
        private string _user = string.Empty;
        private string _password = string.Empty;

        public void Connect(string host, string user, string password)
        {
            _host = host;
            _user = user;
            _password = password;
            _client = new ScpClient(host, user, password);
            _client.Connect();
        }

        public void Close()
        {
            _client?.Disconnect();
            _client?.Dispose();
        }

        public void Copy(string localFilePath, string remoteFileName, SCPCopyDirection direction)
        {
            EnsureConnected();
            try
            {
                if (direction == SCPCopyDirection.LocalToRemote)
                    _client!.Upload(new FileInfo(localFilePath), remoteFileName);
                else
                {
                    using var fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
                    _client!.Download(remoteFileName, fs);
                }
            }
            catch (Exception ex) when (ex.Message.Contains("No such file or directory"))
            {
                throw new NoSuchRemoteFileOrDirectoryException(
                    "No such file or directory: " + remoteFileName, ex);
            }
        }

        private void EnsureConnected()
        {
            if (_client == null || !_client.IsConnected)
                Connect(_host, _user, _password);
        }
    }
}
