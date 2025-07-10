using System.Collections.Concurrent;

namespace MKCards.Common.Models
{
    public class Player
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }

    public class GameServer
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CreatorConnectionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ConcurrentDictionary<string, Player> Players { get; set; } = new();
        public int MaxPlayers { get; set; } = 4;
        public GameState State { get; set; } = GameState.Waiting;
    }

    public enum GameState
    {
        Waiting,
        InProgress,
        Finished
    }

    public class CreateGameRequest
    {
        public string GameName { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public int MaxPlayers { get; set; } = 4;
    }

    public class JoinGameRequest
    {
        public string GameId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
    }

    public class PlayerJoinedResponse
    {
        public bool Success { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public GameServer? GameServer { get; set; }
    }

    public class PlayerLeaveResponse
    {
        public bool Success { get; set; }
        public GameServer? GameServer { get; set; }
    }

    public class GameServerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        public GameServer? GameServer { get; set; }
    }

    public class SendMessageResponse
    {
        public string PlayerName { get; set; } = string.Empty;
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class StartGameResponse
    {
        public GameServer? GameServer { get; set; }
    }
}