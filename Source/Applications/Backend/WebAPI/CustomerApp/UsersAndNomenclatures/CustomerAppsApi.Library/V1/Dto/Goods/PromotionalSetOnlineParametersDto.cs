using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.V1.Dto.Goods
{
	/// <summary>
	/// Параметры промонабора
	/// </summary>
	public class PromotionalSetOnlineParametersDto
	{
		/// <summary>
		/// Идентификатор параметров
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Идентификатор промонабора
		/// </summary>
		public int PromotionalSetId { get; set; }
		/// <summary>
		/// Доступность для продажи
		/// </summary>
		public GoodsOnlineAvailability? AvailableForSale { get; set; }
		/// <summary>
		/// Название промонабора для ИПЗ
		/// </summary>
		public string PromotionalSetOnlineName { get; set; }
		/// <summary>
		/// Промонабор для новых клиентов
		/// </summary>
		public bool PromotionalSetForNewClients { get; set; }
		/// <summary>
		/// Количество бутылей для платной доставки
		/// </summary>
		public int? BottlesCountForCalculatingDeliveryPrice { get; set; }
	}
}
