using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	public static partial class DiscountErrors
	{
		public static class PromoCode
		{
			public static Error NotFound =>
				new Error(
					typeof(DiscountErrors),
					nameof(NotFound),
					"Промокод не найден");

			public static Error ExpiredDateDuration =>
				new Error(
					typeof(DiscountErrors),
					nameof(ExpiredDateDuration),
					"Истекший срок действия");

			public static Error ExpiredTimeDuration(string startTime, string endTime)
			{
				const string message = "Промокод действует только";

				if(string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
				{
					return new Error(typeof(DiscountErrors), nameof(ExpiredTimeDuration), $"{message} по {endTime}");
				}
				
				if(!string.IsNullOrEmpty(startTime) && string.IsNullOrEmpty(endTime))
				{
					return new Error(typeof(DiscountErrors), nameof(ExpiredTimeDuration), $"{message} c {startTime}");
				}
				
				return new Error(typeof(DiscountErrors), nameof(ExpiredTimeDuration), $"{message} c {startTime} по {endTime}");
			}
				

			public static Error InvalidMinimalOrderSum =>
				new Error(
					typeof(DiscountErrors),
					nameof(InvalidMinimalOrderSum),
					"Несоответствие минимальной сумме");
			
			public static Error UnsuitableItemsInCart =>
				new Error(
					typeof(DiscountErrors),
					nameof(UnsuitableItemsInCart),
					"Неподходящие товары в корзине");
			
			public static Error ItemsInCartHasBigDiscount =>
				new Error(
					typeof(DiscountErrors),
					nameof(ItemsInCartHasBigDiscount),
					"Товары в корзине имеют более высокую скидку");
			
			public static Error UsageLimitHasBeenExceeded =>
				new Error(
					typeof(DiscountErrors),
					nameof(UsageLimitHasBeenExceeded),
					"Превышен лимит использований");
		}
	}
}
