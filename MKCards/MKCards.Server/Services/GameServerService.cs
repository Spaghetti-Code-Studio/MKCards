using System.Collections.Concurrent;
using System.Diagnostics;
using MKCards.Common.Models;

namespace MKCards.Server.Services
{
	using PlayerConnectionString = String;
	using GameId = String;

	public interface IGameServerService
	{
		Task<GameServerResponse> CreateGameServerAsync(CreateGameRequest request, PlayerConnectionString connectionId);
		Task<GameServerResponse> JoinGameServerAsync(JoinGameRequest request, PlayerConnectionString connectionId);
		Task<PlayerLeftResponse> LeaveGameServerAsync(PlayerConnectionString connectionId);
		Task<GameServer?> GetGameServerAsync(GameId gameId);
		Task<List<GameServer>> GetAllGameServersAsync();
		Task<GameServer?> GetGameServerByConnectionIdAsync(PlayerConnectionString connectionId);
		Task<bool> RemoveEmptyGameServersAsync();
	}

	public class GameServerService : IGameServerService
	{

		private readonly ConcurrentDictionary<GameId, GameServer> _gameServers = new();
		private readonly ConcurrentDictionary<PlayerConnectionString, GameId> _connectionToGameMap = new();

		private bool IsGameNameUnique(string name) => !_gameServers.Values.Any(gameServer => gameServer.Name == name);

		public async Task<GameServerResponse> CreateGameServerAsync(CreateGameRequest request, string connectionId)
		{
			if (!IsGameNameUnique(request.GameName))
			{
				return new GameServerResponse
				{
					Success = false,
					Message = $"Game server with name `{request.GameName}` has been already created!",
					GameId = string.Empty,
					GameServer = null
				};
			}

			var gameId = GenerateGameId();
			var gameServer = new GameServer
			{
				Id = gameId,
				Name = request.GameName,
				CreatorConnectionId = connectionId,
				MaxPlayers = request.MaxPlayers
			};

			var creator = new Player
			{
				ConnectionId = connectionId,
				Name = request.PlayerName
			};

			gameServer.Players.TryAdd(connectionId, creator);
			_gameServers.TryAdd(gameId, gameServer);
			_connectionToGameMap.TryAdd(connectionId, gameId);

			return new GameServerResponse
			{
				Success = true,
				Message = "Game server created successfully",
				GameId = gameId,
				GameServer = gameServer
			};
		}

		public async Task<GameServerResponse> JoinGameServerAsync(JoinGameRequest request, string connectionId)
		{
			if (!_gameServers.TryGetValue(request.GameId, out var gameServer))
			{
				return new GameServerResponse
				{
					Success = false,
					Message = $"Game server with ID `{request.GameId}` not found!"
				};
			}

			if (gameServer.Players.Count >= gameServer.MaxPlayers)
			{
				return new GameServerResponse
				{
					Success = false,
					Message = $"Game server with ID `{request.GameId}` is full!"
				};
			}

			if (gameServer.State != GameServer.GameState.Waiting)
			{
				return new GameServerResponse
				{
					Success = false,
					Message = $"Game on server with ID `{request.GameId}` is already in progress!"
				};
			}

			bool isNameUniqueInGameServer = !_gameServers[request.GameId].Players.Values.Any(player => player.Name == request.PlayerName);
			if (!isNameUniqueInGameServer)
			{
				return new GameServerResponse
				{
					Success = false,
					Message = $"Player name `{request.PlayerName}` is already present in the game server!",
				};
			}

			var player = new Player
			{
				ConnectionId = connectionId,
				Name = request.PlayerName
			};

			gameServer.Players.TryAdd(connectionId, player);
			_connectionToGameMap.TryAdd(connectionId, request.GameId);

			return new GameServerResponse
			{
				Success = true,
				Message = "Successfully joined game server",
				GameId = request.GameId,
				GameServer = gameServer
			};
		}

		public async Task<PlayerLeftResponse> LeaveGameServerAsync(string connectionId)
		{
			Debug.Assert(connectionId != null);

			if (!_connectionToGameMap.TryGetValue(connectionId, out var gameId))
			{
				return new PlayerLeftResponse
				{
					Success = false,
					GameServer = null
				};
			}

			if (!_gameServers.TryGetValue(gameId, out var gameServer))
			{
				return new PlayerLeftResponse
				{
					Success = false,
					GameServer = null
				};
			}

			gameServer.Players.TryRemove(connectionId, out _);
			_connectionToGameMap.TryRemove(connectionId, out _);

			// Remove game server if empty
			if (gameServer.Players.IsEmpty)
			{
				_gameServers.TryRemove(gameId, out _);
				return new PlayerLeftResponse
				{
					Success = true,
					GameServer = null
				};
			}

			return new PlayerLeftResponse
			{
				Success = true,
				GameServer = gameServer
			};
		}

		public async Task<GameServer?> GetGameServerAsync(string gameId)
		{
			_gameServers.TryGetValue(gameId, out var gameServer);
			return gameServer;
		}

		public async Task<List<GameServer>> GetAllGameServersAsync()
		{
			return _gameServers.Values.ToList();
		}

		public async Task<GameServer?> GetGameServerByConnectionIdAsync(string connectionId)
		{
			if (!_connectionToGameMap.TryGetValue(connectionId, out var gameId))
			{
				return null;
			}

			return await GetGameServerAsync(gameId);
		}

		public async Task<bool> RemoveEmptyGameServersAsync()
		{
			var emptyServers = _gameServers.Where(kvp => kvp.Value.Players.IsEmpty).ToList();
			foreach (var server in emptyServers)
			{
				_gameServers.TryRemove(server.Key, out _);
			}
			return true;
		}

		private string GenerateGameId()
		{
			// Generate a 6-character alphanumeric code
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			return new string(Enumerable.Repeat(chars, 6)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}
	}
}