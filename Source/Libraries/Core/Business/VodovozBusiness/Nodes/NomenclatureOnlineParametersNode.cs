using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Nodes
{
	/// <summary>
	/// Данные онлайн параметров номенклатуры
	/// </summary>
	public class NomenclatureOnlineParametersNode
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
		/// Услуга
		/// </summary>
		public bool IsService { get; set; }
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
