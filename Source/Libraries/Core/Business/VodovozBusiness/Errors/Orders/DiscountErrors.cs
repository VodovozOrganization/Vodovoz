using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	public static partial class DiscountErrors
	{
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
