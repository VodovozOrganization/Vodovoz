using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Заказ в запросе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartOrder
	{
		/// <summary>
		/// Стоимость доставки
		/// </summary>
		[JsonPropertyName("deliveryCost")]
		public decimal DeliveryCost { get; set; }

		/// <summary>
		/// Бонусы, которые клиент хочет списать
		/// </summary>
		[JsonPropertyName("bonusPoints")]
		public List<MindboxAuthorizedCartBonusPoint> BonusPoints { get; set; }

		/// <summary>
		/// Позиции заказа без доставки
		/// </summary>
		[JsonPropertyName("lines")]
		public List<MindboxAuthorizedCartLine> Lines { get; set; }
	}
}
