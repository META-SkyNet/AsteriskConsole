using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Renci.SshNet;

namespace VisualAsterisk.Core.SSH
{
    public class SSHCommand : ISSHCommand
    {
        private SshClient? _sshClient;
        private ScpClient? _scpClient;
        private string _host = string.Empty;
        private string _user = string.Empty;
        private string _password = string.Empty;

        public void Connect(string host, string user, string password)
        {
            _host = host;
            _user = user;
            _password = password;

            _sshClient = new SshClient(host, user, password);
            _sshClient.Connect();

            _scpClient = new ScpClient(host, user, password);
            _scpClient.Connect();
        }

        public void Close()
        {
            _scpClient?.Disconnect();
            _scpClient?.Dispose();
            _sshClient?.Disconnect();
            _sshClient?.Dispose();
        }

        public IList<string> Execute(string cmd)
        {
            if (_sshClient == null || !_sshClient.IsConnected)
                throw new InvalidOperationException("SSH not connected.");

            using var command = _sshClient.RunCommand(cmd);
            if (command.ExitStatus != 0 && command.Error.Contains("command not found"))
                throw new CommandNotFoundException(cmd + ": command not found");

            var result = new List<string>();
            foreach (var line in command.Result.Split('\n'))
            {
                var trimmed = line.TrimEnd('\r');
                if (trimmed.Length > 0)
                    result.Add(trimmed);
            }
            return result;
        }

        public IList<string> ExecuteRedirect(string cmd)
        {
            var tmp = ".ssh_command_output." + DateTime.Now.Ticks;
            Execute(cmd + " > " + tmp);
            Copy(tmp, tmp, SCPCopyDirection.RemoteToLocal);
            Execute("rm -f " + tmp);

            var result = new List<string>();
            try
            {
                foreach (var line in File.ReadAllLines(tmp))
                    result.Add(line);
            }
            catch (Exception e)
            {
                throw new IOException("Could not read redirected output: " + e.Message, e);
            }
            return result;
        }

        public void Copy(string localFilePath, string remoteFileName, SCPCopyDirection direction)
        {
            if (_scpClient == null || !_scpClient.IsConnected)
            {
                _scpClient = new ScpClient(_host, _user, _password);
                _scpClient.Connect();
            }

            if (direction == SCPCopyDirection.LocalToRemote)
            {
                _scpClient.Upload(new FileInfo(localFilePath), remoteFileName);
            }
            else
            {
                using var fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
                _scpClient.Download(remoteFileName, fs);
            }
        }
    }
}
