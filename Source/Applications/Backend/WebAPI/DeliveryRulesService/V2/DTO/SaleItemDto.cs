using System.Text.Json.Serialization;

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
		
		public bool IsNotServiceNomenclature =>
			Type != SaleItemType.PromoSet
			&& Type != SaleItemType.RentPackage
			&& Type != SaleItemType.Service;
	}

	//TODO этот же тип есть в задаче 5942, надо потом объединить их и брать из одного проекта
	public enum SaleItemType
	{
		Water,
		PromoSet,
		RentPackage,
		Service,
		Equipment,
		Other
	}
}
