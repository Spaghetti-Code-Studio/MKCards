using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace MKCards.Client.Services
{
	using PlayerConnectionString = String;
	public class PlayerHubConnection : IAsyncDisposable
	{
		private HubConnection? _hubConnection;
		public PlayerConnectionString? Id => _hubConnection?.ConnectionId;
		public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
		private readonly NavigationManager _navigationManager;

		public PlayerHubConnection(NavigationManager navigationManager)
		{
			_navigationManager = navigationManager;
		}

		public async Task StartConnectionAsync(string url)
		{
			if (_hubConnection is null)
			{
				_hubConnection = new HubConnectionBuilder()
					.WithUrl(_navigationManager.ToAbsoluteUri(url))
					.WithAutomaticReconnect()
					.Build();

				await _hubConnection.StartAsync();
			}
		}

		public async Task SendMessageAsync(string method, params object[] args)
		{
			if (_hubConnection is not null)
			{
				await _hubConnection.SendCoreAsync(method, args);
			}
		}

		public IDisposable RegisterHandler<T>(string methodName, Func<T, Task> handler)
		{
			if (_hubConnection is not null)
			{
				return _hubConnection.On<T>(methodName, handler);
			}

			throw new InvalidOperationException("Hub connection is not initialized.");
		}

		public IDisposable RegisterHandler<T>(string methodName, Action<T> handler)
		{
			if (_hubConnection != null)
			{
				return _hubConnection.On<T>(methodName, handler);
			}

			throw new InvalidOperationException("Connection has not been started yet!");
		}

		public IDisposable RegisterHandler(string methodName, Action handler)
		{
			if (_hubConnection != null)
			{
				return _hubConnection.On(methodName, handler);
			}

			throw new InvalidOperationException("Connection has not been started yet!");
		}

		public HubConnectionState? ConnectionState => _hubConnection?.State;

		public async ValueTask DisposeAsync()
		{
			if (_hubConnection is not null)
			{
				await _hubConnection.DisposeAsync();
			}
		}
	}
}
