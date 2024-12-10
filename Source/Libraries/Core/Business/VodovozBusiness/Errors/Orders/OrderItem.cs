namespace Vodovoz.Errors.Orders
{
	public static partial class OrderItem
	{
		public static Error NotFoundError =>
			new Error(
				typeof(OrderItem),
				nameof(NotFoundError),
				"Строка заказа не найдена");

		public static Error CreateNotFoundError(int orderItemId) =>
			new Error(
				typeof(OrderItem),
				nameof(NotFoundError),
				$"Строка заказа {orderItemId} не найдена");
	}
}
