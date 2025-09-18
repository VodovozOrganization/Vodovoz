using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	public static class OrderItemErrors
	{
		public static Error NotFound =>
			new Error(
				typeof(OrderItemErrors),
				nameof(NotFound),
				"Строка заказа не найдена");
	}
}
