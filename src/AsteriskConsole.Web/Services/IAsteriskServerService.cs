using VisualAsterisk.Asterisk;

namespace AsteriskConsole.Web.Services
{
    public interface IAsteriskServerService
    {
        bool IsConnected { get; }
        IAsteriskServer? Server { get; }
        string? ConnectionError { get; }
        bool HasConfigAccess { get; }
        string? ConfigAccessError { get; }

        Task ConnectAsync(string host, int port, string username, string password);
        void Disconnect();

        IReadOnlyList<AsteriskChannel> GetActiveChannels();
        IReadOnlyList<AsteriskQueue> GetQueues();
        IReadOnlyList<AsteriskPeer> GetPeers();
        IReadOnlyList<ParkedCall> GetParkedCalls();
        IReadOnlyList<AsteriskMeetMeRoom> GetMeetMeRooms();
        IReadOnlyList<CallDetailRecord> GetCallDetailRecords();

        event EventHandler<string> StateChanged;
    }
}
