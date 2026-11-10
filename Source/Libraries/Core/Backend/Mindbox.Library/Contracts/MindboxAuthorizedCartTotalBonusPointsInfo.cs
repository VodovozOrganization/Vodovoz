using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Информация по бонусам заказа в ответе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartTotalBonusPointsInfo
	{
		/// <summary>
		/// Статус расчёта бонусов
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Сколько бонусов можно списать в заказе
		/// </summary>
		[JsonPropertyName("availableAmountForCurrentOrder")]
		public int AvailableAmountForCurrentOrder { get; set; }

		/// <summary>
		/// Сколько бонусов списано в заказе
		/// </summary>
		[JsonPropertyName("spentAmountForCurrentOrder")]
		public int SpentAmountForCurrentOrder { get; set; }

		/// <summary>
		/// Баланс балльного счёта
		/// </summary>
		[JsonPropertyName("balance")]
		public MindboxAuthorizedCartBalanceInfo Balance { get; set; }
	}
}
