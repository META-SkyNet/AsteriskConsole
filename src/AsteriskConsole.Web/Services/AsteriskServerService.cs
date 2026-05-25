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

        public async Task ConnectAsync(string host, int port, string username, string password, int timeoutSeconds = 20)
        {
            try
            {
                Disconnect();
                ConnectionError = null;
                _server = new DefaultAsteriskServer(host, port, username, password);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                try
                {
                    await Task.Run(() => _server.Initialize()).WaitAsync(cts.Token);
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                    _server = null;
                    throw new TimeoutException($"Kết nối timeout sau {timeoutSeconds}s. Kiểm tra lại host/port.");
                }
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

        public async Task<(bool ok, string message)> TestConnectionAsync(string host, int port, int timeoutMs = 3000)
        {
            try
            {
                using var tcp = new System.Net.Sockets.TcpClient();
                await tcp.ConnectAsync(host, port).WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
                var stream = tcp.GetStream();
                stream.ReadTimeout = 2000;
                var buf = new byte[64];
                int n = await stream.ReadAsync(buf, 0, buf.Length).WaitAsync(TimeSpan.FromMilliseconds(2000));
                var banner = System.Text.Encoding.ASCII.GetString(buf, 0, n).Trim();
                return (true, banner);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
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
