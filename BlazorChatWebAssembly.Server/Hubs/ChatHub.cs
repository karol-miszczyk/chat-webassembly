using BlazorChatWebAssembly.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BlazorChatWebAssembly.Server.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> userLookup = new ConcurrentDictionary<string, string>();

        public async Task SendMessage(string username, string message)
        {
            await Clients
                .All
                .SendAsync(Messages.RECEIVE, username, message);
        }

        public async Task Register(string username)
        {
            if (userLookup.TryAdd(Context.ConnectionId, username))
            {
                await Clients
                    .AllExcept(Context.ConnectionId)
                    .SendAsync(Messages.RECEIVE, username, $"{username} joined the chat");
            }
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            if (!userLookup.TryRemove(Context.ConnectionId, out string userName))
                userName = "[unknown]";

            await Clients
                .AllExcept(Context.ConnectionId)
                .SendAsync(Messages.RECEIVE, userName, $"{userName} has left the chat");

            await base.OnDisconnectedAsync(e);
        }
    }
}