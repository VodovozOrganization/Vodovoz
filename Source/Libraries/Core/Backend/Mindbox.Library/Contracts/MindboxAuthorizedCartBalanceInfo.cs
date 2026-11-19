using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Баланс балльного счёта в ответе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartBalanceInfo
	{
		/// <summary>
		/// Всего бонусов
		/// </summary>
		[JsonPropertyName("total")]
		public decimal Total { get; set; }

		/// <summary>
		/// Доступно бонусов
		/// </summary>
		[JsonPropertyName("available")]
		public decimal Available { get; set; }

		/// <summary>
		/// Заблокировано бонусов
		/// </summary>
		[JsonPropertyName("blocked")]
		public decimal Blocked { get; set; }
	}
}
