using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Interfaces.Sale;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Интерфейс для применения скидки к позиции
	/// </summary>
	public interface IApplyDiscountReasonItem : IPrice, IApplicableDiscount, ICalculatingTotalMoneyDiscount
	{
		/// <summary>
		/// Текущее количество
		/// </summary>
		decimal CurrentCount { get; }
		/// <summary>
		/// Данные скидки
		/// </summary>
		IDiscountValue DiscountData { get; }
		/// <summary>
		/// Персональная скидка
		/// </summary>
		new PersonalDiscount PersonalDiscount { get; set; }
		/// <summary>
		/// Список оснований скидок <see cref="Vodovoz.Domain.Orders.DiscountReason"/>
		/// </summary>
		new IList<DiscountReasonBase> DiscountReasons { get; }
		/// <summary>
		/// Установка скидки
		/// </summary>
		/// <param name="discountValue">Данные скидки</param>
		void SetDiscount(IDiscountValue discountValue);
	}
}
