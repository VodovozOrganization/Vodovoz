using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Responses.V1
{
	/// <summary>
	/// Результат вычисления цены заказа
	/// </summary>
	public class CalculatePriceResponse
	{
		/// <summary>
		/// Цена доставки
		/// </summary>
		[JsonPropertyName("delivery_price")]
		public decimal DeliveryPrice { get; set; }

		/// <summary>
		/// Цена заказа
		/// </summary>
		[JsonPropertyName("order_price")]
		public decimal OrderPrice { get; set; }

		/// <summary>
		/// Цена неустойки
		/// </summary>
		[JsonPropertyName("forfeit_price")]
		public decimal ForfeitPrice { get; set; }
	}
}
