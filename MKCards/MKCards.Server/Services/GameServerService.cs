using System.Collections.Concurrent;
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
		Task<GameServerByConnectionIdResponse> GetGameServerAsync(GameId gameId);
		Task<List<GameServer>> GetAllGameServersAsync();
		Task<GameServerByConnectionIdResponse> GetGameServerByConnectionIdAsync(PlayerConnectionString connectionId);
		Task<bool> RemoveEmptyGameServersAsync();
	}

	// TODO: currently, all these methods are async, although C# advices me remove async from them, as they have no await in them - in the future, they might?
	public class GameServerService : IGameServerService
	{
		private readonly ConcurrentDictionary<GameId, GameServer> _gameServers = new();
		private readonly ConcurrentDictionary<PlayerConnectionString, GameId> _connectionToGameMap = new();

		private readonly GameIdGenerator _gameIdGenerator = new(6);

		private bool IsGameNameUnique(string name) => !_gameServers.Values.Any(gameServer => gameServer.Name == name);

		public async Task<GameServerResponse> CreateGameServerAsync(CreateGameRequest request, PlayerConnectionString connectionId)
		{
			if (!IsGameNameUnique(request.GameName))
			{
				return new GameServerResponse
				{
					Success = false,
					Message = $"{(request.Visibility == GameServer.GameServerVisibility.Public ? "Public" : "Private")} game server with name `{request.GameName}` has been already created!",
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
				MaxPlayers = request.MaxPlayers,
				Visibility = request.Visibility
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
				Message = $"{(request.Visibility == GameServer.GameServerVisibility.Public ? "Public" : "Private")} game server `{gameId}` created successfully.",
				GameId = gameId,
				GameServer = gameServer
			};
		}

		public async Task<GameServerResponse> JoinGameServerAsync(JoinGameRequest request, PlayerConnectionString connectionId)
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
				Message = $"Player `{player.Name}` successfully joined game server.",
				GameId = request.GameId,
				GameServer = gameServer
			};
		}

		public async Task<PlayerLeftResponse> LeaveGameServerAsync(PlayerConnectionString connectionId)
		{
			if (!_connectionToGameMap.TryGetValue(connectionId, out var gameId))
			{
				return new PlayerLeftResponse
				{
					Success = false,
					GameServer = null,
					Message = "Unknown connection!"
				};
			}

			if (!_gameServers.TryGetValue(gameId, out var gameServer))
			{
				return new PlayerLeftResponse
				{
					Success = false,
					GameServer = null,
					Message = "Unknown game server!"
				};
			}

			gameServer.Players.TryRemove(connectionId, out _);
			_connectionToGameMap.TryRemove(connectionId, out _);

			if (gameServer.Players.IsEmpty)
			{
				_gameServers.TryRemove(gameId, out _);
				return new PlayerLeftResponse
				{
					Success = true,
					GameServer = null,
					Message = "Player was successfully removed. Game server had become empty and was removed."
				};
			}

			return new PlayerLeftResponse
			{
				Success = true,
				GameServer = gameServer,
				Message = "Player was successfully removed."
			};
		}

		public async Task<GameServerByConnectionIdResponse> GetGameServerAsync(GameId gameId)
		{
			if (!_gameServers.TryGetValue(gameId, out var gameServer))
			{
				return new GameServerByConnectionIdResponse
				{
					Success = false,
					Message = $"Game server with ID `{gameId}` does not exist!"
				};
			}

			return new GameServerByConnectionIdResponse
			{
				Success = true,
				GameServer = gameServer
			};
		}

		public async Task<List<GameServer>> GetAllGameServersAsync() => _gameServers.Values.ToList();

		public async Task<GameServerByConnectionIdResponse> GetGameServerByConnectionIdAsync(PlayerConnectionString connectionId)
		{
			if (!_connectionToGameMap.TryGetValue(connectionId, out var gameId))
			{
				return new GameServerByConnectionIdResponse
				{
					Success = false,
					Message = $"Connection with ID `{connectionId}` does not exist!"
				};
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

		private GameId GenerateGameId()
		{
			return _gameIdGenerator.GenerateId();
		}
	}
}