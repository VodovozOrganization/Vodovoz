using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Тип продаваемой позиции
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum SaleItemType
	{
		/// <summary>
		/// Вода
		/// </summary>
		Water,
		/// <summary>
		/// Оборудование
		/// </summary>
		Equipment,
		/// <summary>
		/// Промонаборы
		/// </summary>
		PromoSet,
		/// <summary>
		/// Пакеты аренды
		/// </summary>
		RentPackage,
		/// <summary>
		/// Услуги
		/// </summary>
		Service,
		/// <summary>
		/// Другое
		/// </summary>
		Other
	}
}
