using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Результат расчёта заказа в ответе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartResultOrder
	{
		/// <summary>
		/// Статус обработки заказа
		/// </summary>
		[JsonPropertyName("processingStatus")]
		public string ProcessingStatus { get; set; }

		/// <summary>
		/// Итоговая стоимость заказа
		/// </summary>
		[JsonPropertyName("totalPrice")]
		public decimal? TotalPrice { get; set; }

		/// <summary>
		/// Информация по бонусам заказа
		/// </summary>
		[JsonPropertyName("totalBonusPointsInfo")]
		public MindboxAuthorizedCartTotalBonusPointsInfo TotalBonusPointsInfo { get; set; }

		/// <summary>
		/// Информация по бонусам в разрезе балльных счетов
		/// </summary>
		[JsonPropertyName("bonusPointsInfoPerBalanceType")]
		public List<MindboxAuthorizedCartTotalBonusPointsInfo> BonusPointsInfoPerBalanceType { get; set; }

		/// <summary>
		/// Изменения бонусов по балльным счетам
		/// </summary>
		[JsonPropertyName("bonusPointsChanges")]
		public List<MindboxAuthorizedCartBonusPointsChange> BonusPointsChanges { get; set; }

		/// <summary>
		/// Позиции заказа в ответе
		/// </summary>
		[JsonPropertyName("lines")]
		public List<MindboxAuthorizedCartResultLine> Lines { get; set; }
	}
}
