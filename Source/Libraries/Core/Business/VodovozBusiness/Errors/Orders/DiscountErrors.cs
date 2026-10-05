using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	/// <summary>
	/// Ошибки, связанные с установкой скидки для строки заказа
	/// </summary>
	public static partial class DiscountErrors
	{
		/// <summary>
		/// Установка скидки для данной позиции не допускается
		/// </summary>
		public static Error DiscountForItemNotAllowed =>
			new Error(
				typeof(DiscountErrors),
				nameof(DiscountForItemNotAllowed),
				"Установка скидки для данной позиции не допускается");
		
		/// <summary>
		/// Скидка не может быть применена
		/// </summary>
		public static Error DiscountNotAllowed =>
			new Error(
				typeof(DiscountErrors),
				nameof(DiscountNotAllowed),
				"Скидка не может быть применена");
		
		/// <summary>
		/// Скидка не найдена
		/// </summary>
		public static Error NotFound =>
			new Error(
				typeof(DiscountErrors),
				nameof(NotFound),
				"Скидка не найдена");
		
		/// <summary>
		/// Сумма позиции равна нулю, скидка не устанавливается
		/// </summary>
		public static Error ZeroSaleItemSum =>
			new Error(
				typeof(DiscountErrors),
				nameof(ZeroSaleItemSum),
				"Установка скидки не допускается: сумма позиции равна нулю");
		
		/// <summary>
		/// Данная скидка уже применена
		/// </summary>
		public static Error DiscountAlreadyApplied =>
			new Error(
				typeof(DiscountErrors),
				nameof(DiscountAlreadyApplied),
				"Скидка уже применена");

		/// <summary>
		/// Строка заказа содержит промонабор или фиксированную цену
		/// </summary>
		public static Error OrderItemContainsPromoSetOrFixedPrice =>
			new Error(
				typeof(DiscountErrors),
				nameof(OrderItemContainsPromoSetOrFixedPrice),
				"Строка заказа содержит промонабор или фиксированную цену");

		/// <summary>
		/// При добавлении скидки произошла непредвиденная ошибка
		/// </summary>
		public static Error AddDiscountException =>
			new Error(
				typeof(DiscountErrors),
				nameof(AddDiscountException),
				"При добавлении скидки произошла непредвиденная ошибка");

		/// <summary>
		/// При добавлении скидки произошла непредвиденная ошибка
		/// </summary>
		/// <param name="message">Сообщение об ошибке</param>
		/// <returns></returns>
		public static Error CreateAddDiscountException(string message) =>
			new Error(
				typeof(DiscountErrors),
				nameof(AddDiscountException),
				$"При добавлении скидки произошла ошибка: {message}");
		
		public static Error UnsuitableItemsInCart =>
			new Error(
				typeof(DiscountErrors),
				nameof(UnsuitableItemsInCart),
				"Неподходящие товары в корзине");
	}
}
