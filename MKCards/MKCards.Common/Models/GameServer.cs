using System.Collections.Concurrent;

namespace MKCards.Common.Models
{
	using PlayerConnectionString = String;
	public class GameServer
	{

		public enum GameState
		{
			Waiting,
			InProgress,
			Finished
		}

		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public PlayerConnectionString CreatorConnectionId { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public ConcurrentDictionary<PlayerConnectionString, Player> Players { get; set; } = new();
		public int MaxPlayers { get; set; } = 4;
		public GameState State { get; set; } = GameState.Waiting;
	}
}