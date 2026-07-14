using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Goods;

namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Данные по товару/услуге из ИПЗ
	/// </summary>
	public class SaleItemDto
	{
		/// <summary>
		/// Идентификатор товара из корзины ИПЗ
		/// </summary>
		//[JsonPropertyOrder(0)]
		public int? ErpId { get; set; }
		
		/// <summary>
		/// Идентификатор товара из корзины ИПЗ
		/// </summary>
		//[JsonPropertyOrder(1)]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public SaleItemType Type { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		//[JsonPropertyOrder(2)]
		public int Amount { get; set; }
		
		/// <summary>
		/// Допустима для ДЗЧ
		/// </summary>
		[JsonIgnore]
		public bool AllowedToFastDelivery =>
			Type != SaleItemType.PromoSet
			&& Type != SaleItemType.RentPackage
			&& Type != SaleItemType.Service;
	}
}
