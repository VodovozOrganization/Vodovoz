using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Изменение бонусов по балльному счёту в ответе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartBonusPointsChange
	{
		/// <summary>
		/// Начислено бонусов
		/// </summary>
		[JsonPropertyName("earnedAmount")]
		public decimal EarnedAmount { get; set; }

		/// <summary>
		/// Списано бонусов
		/// </summary>
		[JsonPropertyName("spentAmount")]
		public decimal SpentAmount { get; set; }

		/// <summary>
		/// Балльный счёт
		/// </summary>
		[JsonPropertyName("balanceType")]
		public MindboxAuthorizedCartResponseBalanceType BalanceType { get; set; }
	}
}
