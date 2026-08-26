using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Интерфейс для применения скидки к позиции
	/// </summary>
	public interface IApplyDiscountReasonItem : IPrice
	{
		/// <summary>
		/// Текущее количество
		/// </summary>
		decimal CurrentCount { get; }
		/// <summary>
		/// Текущая стоимость
		/// </summary>
		decimal CurrentRawPrice { get; }
		/// <summary>
		/// Данные скидки
		/// </summary>
		IDiscountValue DiscountData { get; }
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; }
		/// <summary>
		/// Номенклатура
		/// </summary>
		Nomenclature Nomenclature { get; }
		/// <summary>
		/// Промонабор
		/// </summary>
		PromotionalSet PromoSet { get; }
		/// <summary>
		/// Персональная скидка
		/// </summary>
		PersonalDiscount PersonalDiscount { get; set; }
		/// <summary>
		/// Скидки
		/// </summary>
		IList<DiscountReason> DiscountReasons { get; }
		/// <summary>
		/// Установка скидки
		/// </summary>
		/// <param name="discountValue">Данные скидки</param>
		void SetDiscount(IDiscountValue discountValue);
	}
}
