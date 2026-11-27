using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Балльный счёт
	/// </summary>
	public sealed class MindboxBonusBalance
	{
		/// <summary>
		/// Системное имя балльного счёта
		/// </summary>
		[JsonPropertyName("systemName")]
		public string SystemName { get; set; }
	}
}
