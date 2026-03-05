using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public interface ICalculatingPrice
	{
		/// <summary>
		/// Id сущности
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		bool IsDiscountInMoney { get; }
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; }
		/// <summary>
		/// Основание скидки <see cref="Vodovoz.Domain.Orders.DiscountReason"/>
		/// </summary>
		DiscountReason DiscountReason { get; }
		/// <summary>
		/// Номенклатура <see cref="Vodovoz.Domain.Goods.Nomenclature"/>
		/// </summary>
		Nomenclature Nomenclature { get; }
		/// <summary>
		/// Промо набор <see cref="Vodovoz.Domain.Orders.PromotionalSet"/>
		/// </summary>
		PromotionalSet PromoSet { get; }
	}
}
