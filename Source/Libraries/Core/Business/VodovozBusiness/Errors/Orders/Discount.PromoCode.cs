namespace Vodovoz.Errors.Orders
{
	public static partial class Discount
	{
		public static class PromoCode
		{
			public static Error NotFound =>
				new Error(
					typeof(Discount),
					nameof(NotFound),
					"Промокод не найден");

			public static Error ExpiredDateDuration =>
				new Error(
					typeof(Discount),
					nameof(ExpiredDateDuration),
					"Истекший срок действия");

			public static Error ExpiredTimeDuration(string startTime, string endTime)
			{
				const string message = "Промокод действует только";

				if(string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
				{
					return new Error(typeof(Discount), nameof(ExpiredTimeDuration), $"{message} по {endTime}");
				}
				
				if(!string.IsNullOrEmpty(startTime) && string.IsNullOrEmpty(endTime))
				{
					return new Error(typeof(Discount), nameof(ExpiredTimeDuration), $"{message} c {startTime}");
				}
				
				return new Error(typeof(Discount), nameof(ExpiredTimeDuration), $"{message} c {startTime} по {endTime}");
			}
				

			public static Error InvalidMinimalOrderSum =>
				new Error(
					typeof(Discount),
					nameof(InvalidMinimalOrderSum),
					"Несоответствие минимальной сумме");
			
			public static Error UnsuitableItemsInCart =>
				new Error(
					typeof(Discount),
					nameof(UnsuitableItemsInCart),
					"Неподходящие товары в корзине");
			
			public static Error UsageLimitHasBeenExceeded =>
				new Error(
					typeof(Discount),
					nameof(UsageLimitHasBeenExceeded),
					"Превышен лимит использований");
		}
	}
}
