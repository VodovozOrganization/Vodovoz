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
		/// Проверяет, что скидка была добавлена
		/// </summary>
		/// <param name="discountReason">Основание скидки</param>
		/// <returns>Результат проверки</returns>
		bool IsDiscountReasonAdded(DiscountReason discountReason);

		/// <summary>
		/// Проверяет, что скидка несовместма с уже добавленными скидками в строке
		/// </summary>
		/// <param name="discount">Основание скидки</param>
		/// <returns>Результат проверки</returns>
		bool IsDiscountIncompatibleWithAddedDiscounts(DiscountReason discount);

		/// <summary>
		/// Проверяет, что скидка может быть добавлена, т.е. сумма всех добавленных скидок не превышает цену товара
		/// </summary>
		/// <param name="isDiscountInMoney">Указывает, что скидка задана в денежном выражении</param>
		/// <param name="discount">Размер скидки</param>
		/// <returns>Результат проверки</returns>
		bool IsDiscountValueCanBeAdded(bool isDiscountInMoney, decimal discount);

		/// <summary>
		/// Удаление одной скидки по основанию скидки
		/// </summary>
		/// <param name="discountReasonId">Id основания скидки</param>
		void RemoveDiscount(int discountReasonId);
	}
}
