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
		/// Основание скидки <see cref="Vodovoz.Domain.Orders.DiscountReason"/>
		/// </summary>
		DiscountReason DiscountReason { get; set; }
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
		/// Рекомендация
		/// </summary>
		int? RecomendationId { get; }
	}
}
