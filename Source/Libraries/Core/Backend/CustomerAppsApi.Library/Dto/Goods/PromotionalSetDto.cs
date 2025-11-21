using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.Dto.Goods
{
	/// <summary>
	/// Промонабор
	/// </summary>
	public class PromotionalSetDto
	{
		/// <summary>
		/// Id промонабора в ДВ
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Наименование для ИПЗ
		/// </summary>
		public string OnlineName { get; set; }
		/// <summary>
		/// Доступность для продажи
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public GoodsOnlineAvailability? AvailableForSale { get; set; }
		/// <summary>
		/// Для новых клиентов
		/// </summary>
		public bool ForNewClients { get; set; }
		/// <summary>
		/// Количество бутылей для расчета платной доставки
		/// </summary>
		public int? BottlesCountForCalculatingDeliveryPrice { get; set; }
		/// <summary>
		/// Список номенклатур промонабора
		/// </summary>
		public IList<PromotionalNomenclatureDto> PromotionalNomenclatures { get; set; }
	}
}
