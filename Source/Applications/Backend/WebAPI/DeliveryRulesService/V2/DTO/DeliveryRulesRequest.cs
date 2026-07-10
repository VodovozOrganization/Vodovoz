using System.Text.Json.Serialization;

namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Запрос по правилам доставки
	/// </summary>
	public class DeliveryRulesRequest
	{
		/// <summary>
		/// Долгота
		/// </summary>
		public decimal Latitude { get; set; }
		/// <summary>
		/// Широта
		/// </summary>
		public decimal Longitude { get; set; }
		/// <summary>
		/// Корзина
		/// </summary>
		public SaleItemDto[] SaleItems { get; set; }
	}
}
