using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Баланс по одному балльному счёту
	/// </summary>
	public sealed class MindboxBalanceHistoryItem
	{
		[JsonPropertyName("total")]
		public decimal Total { get; set; }

		[JsonPropertyName("available")]
		public decimal Available { get; set; }

		[JsonPropertyName("blocked")]
		public decimal Blocked { get; set; }

		[JsonPropertyName("systemName")]
		public string SystemName { get; set; }

		[JsonPropertyName("nearestExpiration")]
		public MindboxBalanceNearestExpiration NearestExpiration { get; set; }

		[JsonPropertyName("balanceType")]
		public MindboxAuthorizedCartBalanceType BalanceType { get; set; }
	}
}
