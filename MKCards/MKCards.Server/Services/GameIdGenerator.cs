using System.Security.Cryptography;
using System.Text;

namespace MKCards.Server.Services
{
	public class GameIdGenerator
	{
		private static readonly char[] Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
		private int _gameIdLength = 6;

		public GameIdGenerator(int idLength)
		{
			GameIdLength = idLength;
		}

		public int GameIdLength { get => _gameIdLength; private set => _gameIdLength = value; }

		public string GenerateId()
		{
			using var rng = RandomNumberGenerator.Create();
			var bytes = new byte[GameIdLength];
			rng.GetBytes(bytes);

			var result = new StringBuilder(GameIdLength);
			foreach (var byteValue in bytes)
			{
				result.Append(Chars[byteValue % Chars.Length]);
			}

			return result.ToString();
		}
	}
}
