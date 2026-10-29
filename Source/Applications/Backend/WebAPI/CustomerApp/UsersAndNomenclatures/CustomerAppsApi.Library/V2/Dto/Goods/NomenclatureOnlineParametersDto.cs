using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Данные онлайн параметров номенклатуры
	/// </summary>
	public class NomenclatureOnlineParametersDto
	{
		/// <summary>
		/// Идентификатор параметра
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		public int NomenclatureId { get; set; }
		/// <summary>
		/// Тип номенклатуры
		/// </summary>
		public NomenclatureCategory Category { get; set; }
		/// <summary>
		/// Доступность для продажи
		/// </summary>
		public GoodsOnlineAvailability? AvailableForSale { get; set; }
		/// <summary>
		/// Маркер(товар недели, акция и т.д.)
		/// </summary>
		public NomenclatureOnlineMarker? Marker { get; set; }
		/// <summary>
		/// Скидка в процентах
		/// </summary>
		public decimal? PercentDiscount { get; set; }
	}
}
