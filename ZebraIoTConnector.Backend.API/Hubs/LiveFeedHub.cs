using Microsoft.AspNetCore.SignalR;

namespace ZebraIoTConnector.Backend.API.Hubs
{
    public class LiveFeedHub : Hub
    {
        public Task SubscribeToLiveFeed()
        {
            // Client subscribes - can be used for tracking connected clients if needed
            return Task.CompletedTask;
        }

        public Task UnsubscribeFromLiveFeed()
        {
            // Client unsubscribes - can be used for tracking connected clients if needed
            return Task.CompletedTask;
        }
    }
}

