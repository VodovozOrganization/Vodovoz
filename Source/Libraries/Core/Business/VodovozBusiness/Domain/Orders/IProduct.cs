using QS.Extensions.Observable.Collections.List;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	public interface IProduct
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
		/// Скидка
		/// </summary>
		decimal GetDiscount { get; }
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		bool IsDiscountInMoney { get; }
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; }
		/// <summary>
		/// Основания скидок <see cref="Vodovoz.Domain.Orders.DiscountReason"/>
		/// </summary>
		IObservableList<DiscountReason> DiscountReasons { get; }
		/// <summary>
		/// Номенклатура <see cref="Vodovoz.Domain.Goods.Nomenclature"/>
		/// </summary>
		Nomenclature Nomenclature { get; }
		/// <summary>
		/// Промо набор <see cref="Vodovoz.Domain.Orders.PromotionalSet"/>
		/// </summary>
		PromotionalSet PromoSet { get; set; }
		/// <summary>
		/// Фактическая сумма
		/// </summary>
		decimal ActualSum { get; }
		/// <summary>
		/// Текущее количество
		/// </summary>
		decimal CurrentCount { get; }
		/// <summary>
		/// Цена
		/// </summary>
		decimal Price { get; }
		/// <summary>
		/// Этот товар - подарок?
		/// </summary>
		bool IsGift { get; }
	}
}
