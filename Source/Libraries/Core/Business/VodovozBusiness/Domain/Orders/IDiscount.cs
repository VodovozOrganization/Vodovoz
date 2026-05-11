using QS.Extensions.Observable.Collections.List;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Интерфейс для работы со скидками в строке заказа
	/// </summary>
	public interface IDiscount
	{
		/// <summary>
		/// Номенклатура
		/// </summary>
		Nomenclature Nomenclature { get; }

		/// <summary>
		/// Цена товара
		/// </summary>
		decimal Price { get; }

		/// <summary>
		/// Текущее количество
		/// </summary>
		decimal CurrentCount { get; }

		/// <summary>
		/// Указывает, что скидка задана в денежном выражении, а не в процентах
		/// </summary>
		bool IsDiscountInMoney { get; }

		IObservableList<DiscountReason> DiscountReasons { get; }

		/// <summary>
		/// Добавить скидку
		/// </summary>
		/// <param name="isDiscountInMoney"></param>
		/// <param name="discount"></param>
		/// <param name="discountReason"></param>
		void AddDiscount(bool isDiscountInMoney, decimal discount, DiscountReason discountReason);

		/// <summary>
		/// Очистка всех скидок
		/// </summary>
		void ClearDiscounts();

		/// <summary>
		/// Удаление одной скидки по основанию скидки
		/// </summary>
		/// <param name="discountReasonId">Id основания скидки</param>
		void RemoveDiscount(int discountReasonId);
	}
}
