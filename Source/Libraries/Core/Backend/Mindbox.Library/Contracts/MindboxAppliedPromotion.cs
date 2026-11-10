using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Применённая к позиции акция в ответе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAppliedPromotion
	{
		/// <summary>
		/// Тип акции
		/// </summary>
		[JsonPropertyName("type")]
		public string Type { get; set; }

		/// <summary>
		/// Сумма акции
		/// </summary>
		[JsonPropertyName("amount")]
		public decimal Amount { get; set; }

		/// <summary>
		/// Сумма списанных бонусов
		/// </summary>
		[JsonPropertyName("spentBonusPointsAmount")]
		public decimal? SpentBonusPointsAmount { get; set; }
	}
}
