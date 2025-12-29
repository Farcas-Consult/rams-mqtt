using Microsoft.AspNetCore.SignalR;
using ZebraIoTConnector.Backend.API.Hubs;
using ZebraIoTConnector.Services;

namespace ZebraIoTConnector.Backend.API.Services
{
    public class SignalRTagReadNotifier : ITagReadNotifier
    {
        private readonly IHubContext<LiveFeedHub> hubContext;

        public SignalRTagReadNotifier(IHubContext<LiveFeedHub> hubContext)
        {
            this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task NotifyTagReadAsync(object tagReadData)
        {
            await hubContext.Clients.All.SendAsync("TagRead", tagReadData);
        }
    }
}
