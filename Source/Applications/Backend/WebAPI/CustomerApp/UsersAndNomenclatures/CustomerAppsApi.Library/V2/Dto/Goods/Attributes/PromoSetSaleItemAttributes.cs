using System.Collections.Generic;

namespace CustomerAppsApi.Library.V2.Dto.Goods.Attributes
{
	public class PromoSetSaleItemAttributes
	{
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
