using Microsoft.AspNetCore.SignalR;
using MKCards.Common.Models;
using MKCards.Server.Services;

namespace MKCards.Server.Hubs
{
	public class GameHub : Hub
	{
		private readonly IGameServerService _gameServerService;

		public GameHub(IGameServerService gameServerService)
		{
			_gameServerService = gameServerService;
		}

		public async Task CreateGameServer(CreateGameRequest request)
		{
			var response = await _gameServerService.CreateGameServerAsync(request, Context.ConnectionId);

			if (response.Success)
			{
				// Join the creator to their game group
				await Groups.AddToGroupAsync(Context.ConnectionId, response.GameId);

				// Notify the creator
				await Clients.Caller.SendAsync("GameCreated", response);

				// Notify all clients about the new game server
				await Clients.All.SendAsync("GameServerListUpdated", await _gameServerService.GetAllGameServersAsync());
			}
			else
			{
				await Clients.Caller.SendAsync("Error", response.Message);
			}
		}

		public async Task JoinGameServer(JoinGameRequest request)
		{
			var response = await _gameServerService.JoinGameServerAsync(request, Context.ConnectionId);

			if (response.Success)
			{
				// Add the player to the game group
				await Groups.AddToGroupAsync(Context.ConnectionId, request.GameId);

				// Notify the joining player
				await Clients.Caller.SendAsync("GameJoined", response);

				// Notify all players in the game about the new player
				await Clients.Group(request.GameId).SendAsync("PlayerJoined", new
				{
					Success = true,
					PlayerId = Context.ConnectionId,
					PlayerName = request.PlayerName,
					GameServer = response.GameServer
				});

				// Update the game server list for all clients
				await Clients.All.SendAsync("GameServerListUpdated", await _gameServerService.GetAllGameServersAsync());
			}
			else
			{
				await Clients.Caller.SendAsync("Error", response.Message);
			}
		}

		public async Task LeaveGameServer()
		{
			var gameServer = await _gameServerService.GetGameServerByConnectionIdAsync(Context.ConnectionId);

			if (gameServer != null)
			{
				// Remove from game group
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameServer.Id);

				// Remove from service
				var response = await _gameServerService.LeaveGameServerAsync(Context.ConnectionId);

				// Notify remaining players
				await Clients.Group(gameServer.Id).SendAsync("PlayerLeft", new
				{
					Success = response.Success,
					GameServer = response.GameServer
				});

				// Update the game server list for all clients
				await Clients.All.SendAsync("GameServerListUpdated", await _gameServerService.GetAllGameServersAsync());
			}
		}

		public async Task SendMessageToGame(string message)
		{
			var gameServer = await _gameServerService.GetGameServerByConnectionIdAsync(Context.ConnectionId);

			if (gameServer != null && gameServer.Players.TryGetValue(Context.ConnectionId, out var player))
			{
				await Clients.Group(gameServer.Id).SendAsync("GameMessage", new
				{
					Success = true,
					PlayerName = player.Name,
					Message = message,
					Timestamp = DateTime.UtcNow
				});
			}
		}

		public async Task StartGame()
		{
			var gameServer = await _gameServerService.GetGameServerByConnectionIdAsync(Context.ConnectionId);

			if (gameServer != null && gameServer.CreatorConnectionId == Context.ConnectionId)
			{
				gameServer.State = GameServer.GameState.InProgress;

				await Clients.Group(gameServer.Id).SendAsync("GameStarted", new GameStartedResponse
				{
					Success = true,
					GameServer = gameServer
				});

				// Update the game server list for all clients
				await Clients.All.SendAsync("GameServerListUpdated", await _gameServerService.GetAllGameServersAsync());
			}
		}

		public async Task IncrementCounter()
		{
			var id = Context.ConnectionId;
			var gameServer = await _gameServerService.GetGameServerByConnectionIdAsync(Context.ConnectionId);
			await Clients.Group(gameServer.Id).SendAsync("ChangeIncrement");
		}

		public async Task<GameServer?> GetGameServerByConnectionIdAsync(string id)
		{
			return await _gameServerService.GetGameServerByConnectionIdAsync(id);
		}

		public async Task GetGameServerList()
		{
			var gameServers = await _gameServerService.GetAllGameServersAsync();
			await Clients.Caller.SendAsync("GameServerListUpdated", gameServers);
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			await LeaveGameServer();
			await base.OnDisconnectedAsync(exception);
		}
	}
}