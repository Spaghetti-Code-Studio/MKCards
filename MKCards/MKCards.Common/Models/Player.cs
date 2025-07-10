namespace MKCards.Common.Models
{
	using PlayerConnectionString = String;
	public class Player
	{
		public PlayerConnectionString ConnectionId { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
	}
}
