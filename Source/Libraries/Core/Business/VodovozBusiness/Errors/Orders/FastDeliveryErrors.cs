using System;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Extensions;

namespace Vodovoz.Errors.Orders
{
	public static class FastDeliveryErrors
	{
		public static Error Validation =>
			new Error(
				typeof(FastDeliveryErrors),
				nameof(Validation),
				"Произошла ошибка валидации доставки за час для заказа.");

		public static Error RouteListForFastDeliveryIsMissing =>
			new Error(
				typeof(FastDeliveryErrors),
				nameof(RouteListForFastDeliveryIsMissing),
				"Не указан маршрутный лист для доставки на час.");
		
		public static Error RouteListForFastDeliveryNotOnTheWay(int routeListId, RouteListStatus routeListStatus) =>
			new Error(
				typeof(FastDeliveryErrors),
				nameof(RouteListForFastDeliveryIsMissing),
				$"Маршрутный лист №{routeListId} уже не в статусе В пути, а в статусе {routeListStatus.GetEnumDisplayName()}");

		public static Error InvalidDate =>
			new Error(
				typeof(FastDeliveryErrors),
				nameof(InvalidDate),
				"Доставка за час возможна только на текущую дату");

		/// <summary>
		/// Не использовать
		/// Создавать через <seealso cref="CreateInvalidPaymentTypeError"/>
		/// </summary>
		public static Error InvalidPaymentType => throw new NotImplementedException();

		public static Error CreateInvalidPaymentTypeError(PaymentType paymentType) =>
			new Error(
				typeof(FastDeliveryErrors),
				nameof(InvalidPaymentType),
				$"Нельзя выбрать доставку за час для заказа с формой оплаты {paymentType.GetEnumDisplayName()}");

		public static Error DeliveryPointIsMissing =>
			new Error(
				typeof(FastDeliveryErrors),
				nameof(DeliveryPointIsMissing),
				"Перед выбором доставки за час необходимо выбрать точку доставки");

		public static Error Water19LIsMissing =>
			new Error(
				typeof(FastDeliveryErrors),
				nameof(Water19LIsMissing),
				"В заказе с доставкой за час обязательно должна быть 19л вода");
		
		public static Error FastDeliveryIsMissing =>
			new Error(
				typeof(FastDeliveryErrors),
				nameof(FastDeliveryIsMissing),
				"В заказе с доставкой за час обязательно должна быть номенклатура с экспресс доставкой");
		
		public static Error NotNeedFastDelivery =>
			new Error(
				typeof(FastDeliveryErrors),
				nameof(NotNeedFastDelivery),
				"В заказе без доставки за час не должно быть номенклатуры с экспресс доставкой");
	}
}
