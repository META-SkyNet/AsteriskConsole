using Microsoft.AspNetCore.SignalR;
using AsteriskConsole.Web.Services;

namespace AsteriskConsole.Web.Hubs
{
    public class AsteriskHub : Hub
    {
        private readonly IAsteriskServerService _asteriskService;

        public AsteriskHub(IAsteriskServerService asteriskService)
        {
            _asteriskService = asteriskService;
        }

        public async Task GetChannels()
        {
            var channels = _asteriskService.GetActiveChannels()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    CallerIdNum = c.Account ?? string.Empty,
                    CallerIdName = string.Empty,
                    State = c.State.ToString(),
                })
                .ToList();

            await Clients.Caller.SendAsync("ReceiveChannels", channels);
        }

        public async Task GetQueues()
        {
            var queues = _asteriskService.GetQueues()
                .Select(q => new
                {
                    q.Name,
                    EntryCount = q.Entries?.Count ?? 0,
                    MemberCount = q.Members?.Count ?? 0,
                })
                .ToList();

            await Clients.Caller.SendAsync("ReceiveQueues", queues);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
