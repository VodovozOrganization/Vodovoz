using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Списание бонусов в запросе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartBonusPoint
	{
		/// <summary>
		/// Сумма списания
		/// </summary>
		[JsonPropertyName("amount")]
		public int Amount { get; set; }

		/// <summary>
		/// Балльный счёт
		/// </summary>
		[JsonPropertyName("balanceType")]
		public MindboxAuthorizedCartBalanceType BalanceType { get; set; }
	}
}
