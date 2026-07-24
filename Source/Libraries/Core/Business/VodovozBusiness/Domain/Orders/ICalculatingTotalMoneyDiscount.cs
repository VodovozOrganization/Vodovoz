using System.Collections.Generic;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Контракт для расчета итоговой скидки в деньгах
	/// </summary>
	public interface ICalculatingTotalMoneyDiscount
	{
		/// <summary>
		/// Текущая цена по прайсу
		/// </summary>
		decimal CurrentRawPrice { get; }
		/// <summary>
		/// Список оснований скидок
		/// </summary>
		IEnumerable<DiscountReason> DiscountReasons { get; }
	}
}
