using System;

namespace Vodovoz.Errors.Orders
{
	public static partial class Order
	{
		public static class FastDelivery
		{
			public static Error Validation =>
				new Error(
					typeof(FastDelivery),
					nameof(Validation),
					"Произошла ошибка валидации доставки за час для заказа.");

			public static Error RouteListForFastDeliveryIsMissing =>
				new Error(
					typeof(FastDelivery),
					nameof(RouteListForFastDeliveryIsMissing),
					"Не указан маршрутный лист для доставки на час.");

			public static Error InvalidDate =>
				new Error(
					typeof(FastDelivery),
					nameof(InvalidDate),
					"Доставка за час возможна только на текущую дату");

			/// <summary>
			/// Не использовать
			/// Создавать через <seealso cref="CreateInvalidPaymentTypeError"/>
			/// </summary>
			public static Error InvalidPaymentType => throw new NotImplementedException();

			public static Error CreateInvalidPaymentTypeError(string paymentType) =>
				new Error(
					typeof(FastDelivery),
					nameof(InvalidPaymentType),
					$"Нельзя выбрать доставку за час для заказа с формой оплаты {paymentType}");

			public static Error DeliveryPointIsMissing =>
				new Error(
					typeof(FastDelivery),
					nameof(DeliveryPointIsMissing),
					"Перед выбором доставки за час необходимо выбрать точку доставки");

			public static Error Water19LIsMissing =>
				new Error(
					typeof(FastDelivery),
					nameof(Water19LIsMissing),
					"В заказе с доставкой за час обязательно должна быть 19л вода");
		}
	}
}
