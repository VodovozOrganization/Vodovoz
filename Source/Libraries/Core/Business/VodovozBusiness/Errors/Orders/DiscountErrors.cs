using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	public static partial class DiscountErrors
	{
		public static Error DiscountForOrderItemNotAllowed =>
			new Error(
				typeof(DiscountErrors),
				nameof(DiscountForOrderItemNotAllowed),
				"Установка скидки для данной строки заказа не допускается");

		public static Error OrderItemContainsPromoSetOrFixedPrice =>
			new Error(
				typeof(DiscountErrors),
				nameof(OrderItemContainsPromoSetOrFixedPrice),
				"Строка заказа содержит промонабор или фиксированную цену");

		public static Error AddDiscountException =>
			new Error(
				typeof(DiscountErrors),
				nameof(AddDiscountException),
				"При добавлении скидки произошла непредвиденная ошибка");

		public static Error CreateAddDiscountException(string message) =>
			new Error(
				typeof(DiscountErrors),
				nameof(AddDiscountException),
				$"При добавлении скидки произошла ошибка: {message}");
	}
}
