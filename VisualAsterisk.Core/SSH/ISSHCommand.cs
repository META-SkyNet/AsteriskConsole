using System.Collections.Generic;

namespace VisualAsterisk.Core.SSH
{
    public interface ISSHCommand
    {
        void Connect(string host, string user, string password);
        void Close();
        IList<string> Execute(string cmd);
        IList<string> ExecuteRedirect(string cmd);
        void Copy(string localFilePath, string remoteFileName, SCPCopyDirection direction);
    }
}
