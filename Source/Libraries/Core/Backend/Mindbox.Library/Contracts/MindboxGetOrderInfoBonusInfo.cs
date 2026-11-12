using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Бонусная информация по балльному счёту в ответе GetOrderInfo
	/// </summary>
	public sealed class MindboxGetOrderInfoBonusInfo
	{
		[JsonPropertyName("spentAmount")]
		public decimal SpentAmount { get; set; }

		[JsonPropertyName("earnedAmount")]
		public decimal EarnedAmount { get; set; }

		[JsonPropertyName("balanceType")]
		public MindboxAuthorizedCartBalanceType BalanceType { get; set; }
	}
}
