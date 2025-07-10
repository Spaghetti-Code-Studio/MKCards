using System.Collections.Concurrent;
using System.Diagnostics;
using MKCards.Common.Models;

namespace MKCards.Server.Services
{
    public interface IGameServerService
    {
        Task<GameServerResponse> CreateGameServerAsync(CreateGameRequest request, string connectionId);
        Task<GameServerResponse> JoinGameServerAsync(JoinGameRequest request, string connectionId);
        Task<PlayerLeaveResponse> LeaveGameServerAsync(string connectionId);
        Task<GameServer?> GetGameServerAsync(string gameId);
        Task<List<GameServer>> GetAllGameServersAsync();
        Task<GameServer?> GetGameServerByConnectionIdAsync(string connectionId);
        Task<bool> RemoveEmptyGameServersAsync();
    }

    public class GameServerService : IGameServerService
    {
        private readonly ConcurrentDictionary<string, GameServer> _gameServers = new();
        private readonly ConcurrentDictionary<string, string> _connectionToGameMap = new();

        public async Task<GameServerResponse> CreateGameServerAsync(CreateGameRequest request, string connectionId)
        {
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
                    Message = "Game server not found"
                };
            }

            if (gameServer.Players.Count >= gameServer.MaxPlayers)
            {
                return new GameServerResponse
                {
                    Success = false,
                    Message = "Game server is full"
                };
            }

            if (gameServer.State != GameState.Waiting)
            {
                return new GameServerResponse
                {
                    Success = false,
                    Message = "Game is already in progress"
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

        public async Task<PlayerLeaveResponse> LeaveGameServerAsync(string connectionId)
        {
            Debug.Assert(connectionId != null);

            if (!_connectionToGameMap.TryGetValue(connectionId, out var gameId))
            {
                return new PlayerLeaveResponse
                {
                    Success = false,
                    GameServer = null
                };
            }

            if (!_gameServers.TryGetValue(gameId, out var gameServer))
            {
                return new PlayerLeaveResponse
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
                return new PlayerLeaveResponse
                {
                    Success = true,
                    GameServer = null
                };
            }

            return new PlayerLeaveResponse
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