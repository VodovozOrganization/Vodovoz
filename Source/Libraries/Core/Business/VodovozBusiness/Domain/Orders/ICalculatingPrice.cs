using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public interface ICalculatingPrice : INomenclatureCount
	{
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; }
		/// <summary>
		/// Основание скидки
		/// </summary>
		DiscountReason DiscountReason { get; }
		/// <summary>
		/// Промо набор <see cref="Vodovoz.Domain.Orders.PromotionalSet"/>
		/// </summary>
		PromotionalSet PromoSet { get; }
	}
	
	public interface INomenclatureCount
	{
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Номенклатура <see cref="Vodovoz.Domain.Goods.Nomenclature"/>
		/// </summary>
		Nomenclature Nomenclature { get; }
	}
}
