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
				await Groups.AddToGroupAsync(Context.ConnectionId, response.GameId);
				await Clients.Caller.SendAsync("GameCreated", response);
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
				await Groups.AddToGroupAsync(Context.ConnectionId, request.GameId);
				await Clients.Caller.SendAsync("GameJoined", response);
				await Clients.Group(request.GameId).SendAsync("PlayerJoined", new PlayerJoinedResponse
				{
					Success = true,
					PlayerId = Context.ConnectionId,
					PlayerName = request.PlayerName,
					GameServer = response.GameServer
				});

				await Clients.All.SendAsync("GameServerListUpdated", await _gameServerService.GetAllGameServersAsync());
			}
			else
			{
				await Clients.Caller.SendAsync("Error", response.Message);
			}
		}

		public async Task LeaveGameServer()
		{
			var result = await _gameServerService.GetGameServerByConnectionIdAsync(Context.ConnectionId);

			if (result.Success && result.GameServer is not null)
			{
				GameServer gameServer = result.GameServer;
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameServer.Id);

				var response = await _gameServerService.LeaveGameServerAsync(Context.ConnectionId);

				await Clients.Group(gameServer.Id).SendAsync("PlayerLeft", new PlayerLeftResponse
				{
					Success = response.Success,
					GameServer = response.GameServer
				});

				await Clients.All.SendAsync("GameServerListUpdated", await _gameServerService.GetAllGameServersAsync());
			}
			else
			{
				await Clients.Caller.SendAsync("Error", result.Message);
			}
		}

		public async Task SendMessageToGame(string message)
		{
			var result = await _gameServerService.GetGameServerByConnectionIdAsync(Context.ConnectionId);

			// TODO: we might want to differentiate between two different errors: one coming from GetGameServerByConnectionIdAsync(), the second is the check below.
			if (result.Success && result.GameServer is not null && result.GameServer.Players.TryGetValue(Context.ConnectionId, out var player))
			{
				await Clients.Group(result.GameServer.Id).SendAsync("GameMessage", new MessageSentResponse
				{
					Success = true,
					PlayerName = player.Name,
					Message = message,
					TimeStamp = DateTime.UtcNow
				});
			}
			else
			{
				await Clients.Caller.SendAsync("Error", result.Message);
			}
		}

		public async Task StartGame()
		{
			var result = await _gameServerService.GetGameServerByConnectionIdAsync(Context.ConnectionId);

			// TODO: we might want to differentiate between two different errors: one coming from GetGameServerByConnectionIdAsync(), the second is the check below.
			if (result.Success && result.GameServer is not null && result.GameServer.CreatorConnectionId == Context.ConnectionId)
			{
				result.GameServer.State = GameServer.GameState.InProgress;

				await Clients.Group(result.GameServer.Id).SendAsync("GameStarted", new GameStartedResponse
				{
					Success = true,
					GameServer = result.GameServer
				});

				await Clients.All.SendAsync("GameServerListUpdated", await _gameServerService.GetAllGameServersAsync());
			}
			else
			{
				await Clients.Caller.SendAsync("Error", result.Message);
			}
		}

		public async Task IncrementCounter()
		{
			var result = await _gameServerService.GetGameServerByConnectionIdAsync(Context.ConnectionId);
			if (result.Success && result.GameServer is not null)
			{
				await Clients.Group(result.GameServer.Id).SendAsync("ChangeIncrement");
			}
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