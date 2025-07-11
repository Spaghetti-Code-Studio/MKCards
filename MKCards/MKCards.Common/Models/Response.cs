namespace MKCards.Common.Models
{
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
		public string PlayerId { get; set; } = string.Empty;
		public string PlayerName { get; set; } = string.Empty;
		public GameServer? GameServer { get; set; }
	}

	public class PlayerLeftResponse
	{
		public bool Success { get; set; }
		public GameServer? GameServer { get; set; }
		public string Message { get; set; } = string.Empty;
	}

	public class GameServerResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public string GameId { get; set; } = string.Empty;
		public GameServer? GameServer { get; set; }
	}

	public class MessageSentResponse
	{
		public bool Success { get; set; }
		public string PlayerName { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public DateTime TimeStamp { get; set; }
	}

	public class GameStartedResponse
	{
		public bool Success { get; set; }
		public GameServer? GameServer { get; set; }
	}

	public class GameServerByConnectionIdResponse
	{
		public bool Success { get; set; }
		public GameServer? GameServer { get; set; }
		public string Message { get; set; } = string.Empty;
	}
}
