using VisualAsterisk.Asterisk;

namespace AsteriskConsole.Web.Services
{
    public class AsteriskServerService : IAsteriskServerService, IDisposable
    {
        private DefaultAsteriskServer? _server;
        private readonly ILogger<AsteriskServerService> _logger;

        public bool IsConnected => _server?.IsConnected() == true;
        public IAsteriskServer? Server => _server;
        public string? ConnectionError { get; private set; }
        public bool HasConfigAccess => _server?.ConfigLoadError == null && IsConnected;
        public string? ConfigAccessError => _server?.ConfigLoadError;

        public event EventHandler<string>? StateChanged;

        public AsteriskServerService(ILogger<AsteriskServerService> logger)
        {
            _logger = logger;
        }

        public async Task ConnectAsync(string host, int port, string username, string password)
        {
            try
            {
                Disconnect();
                ConnectionError = null;
                _server = new DefaultAsteriskServer(host, port, username, password);
                await Task.Run(() => _server.Initialize());
                _logger.LogInformation("Connected to Asterisk at {Host}:{Port}", host, port);
                StateChanged?.Invoke(this, "Connected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Asterisk at {Host}", host);
                ConnectionError = ex.Message;
                _server = null;
                StateChanged?.Invoke(this, "ConnectionFailed");
            }
        }

        public void Disconnect()
        {
            if (_server != null)
            {
                try { _server.Shutdown(); } catch { }
                _server = null;
                StateChanged?.Invoke(this, "Disconnected");
            }
        }

        public IReadOnlyList<AsteriskChannel> GetActiveChannels()
        {
            try { return _server?.Channels?.ToList() ?? []; }
            catch { return []; }
        }

        public IReadOnlyList<AsteriskQueue> GetQueues()
        {
            try { return _server?.Queues?.ToList() ?? []; }
            catch { return []; }
        }

        public IReadOnlyList<AsteriskPeer> GetPeers()
        {
            try { return _server?.GetPeerEntriesEx()?.ToList() ?? []; }
            catch { return []; }
        }

        public IReadOnlyList<ParkedCall> GetParkedCalls()
        {
            try { return _server?.GetParkedCalls()?.ToList() ?? []; }
            catch { return []; }
        }

        public IReadOnlyList<AsteriskMeetMeRoom> GetMeetMeRooms()
        {
            try { return _server?.MeetmeRooms?.ToList() ?? []; }
            catch { return []; }
        }

        public IReadOnlyList<CallDetailRecord> GetCallDetailRecords()
        {
            // CDR loaded via LoadCDR(directory, reload) on IAsteriskServer.
            // Return from Backups or empty until caller triggers a load.
            return [];
        }

        public void Dispose() => Disconnect();
    }
}
