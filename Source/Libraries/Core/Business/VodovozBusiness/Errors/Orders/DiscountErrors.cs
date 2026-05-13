using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	/// <summary>
	/// Ошибки, связанные с установкой скидки для строки заказа
	/// </summary>
	public static partial class DiscountErrors
	{
		/// <summary>
		/// Установка скидки для данной строки заказа не допускается
		/// </summary>
		public static Error DiscountForOrderItemNotAllowed =>
			new Error(
				typeof(DiscountErrors),
				nameof(DiscountForOrderItemNotAllowed),
				"Установка скидки для данной строки заказа не допускается");

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
	}
}
