using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace MKCards.Client.Services
{
    public class PlayerHubConnection : IAsyncDisposable
    {
        public HubConnection? HubConnection;
        private readonly NavigationManager _navigationManager;

        public PlayerHubConnection(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public async Task StartConnectionAsync()
        {
            if (HubConnection is null)
            {
                HubConnection = new HubConnectionBuilder()
                    .WithUrl(_navigationManager.ToAbsoluteUri("/gamehub"))
                    .WithAutomaticReconnect()
                    .Build();

                await HubConnection.StartAsync();
            }
        }

        public async Task SendMessageAsync(string method, params object[] args)
        {
            if (HubConnection is not null)
            {
                await HubConnection.SendAsync(method, args);
            }
        }

        //public Task On<T>(string methodName, Action<T> handler)
        //{
        //    return await _hubConnection.On(methodName, handler);
        //}

        public HubConnectionState? ConnectionState => HubConnection?.State;

        public async ValueTask DisposeAsync()
        {
            if (HubConnection is not null)
            {
                await HubConnection.DisposeAsync();
            }
        }
    }
}
